using System;
using System.Collections.Generic;
using System.IO;
using AstFramework;
using Ssis2008Emitter.IR.Common;
using Ssis2008Emitter.IR.Tasks;
using Vulcan.Utility.Files;
using VulcanEngine.Common;
using AST = VulcanEngine.IR.Ast;
using DTS = Microsoft.SqlServer.Dts.Runtime;

namespace Ssis2008Emitter.IR.Framework
{
    using DTS;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Physical emission objects are treated as tree nodes and not as collections.")]
    public class Package : Container
    {
        private const string PackagePathRootVariableName = "_ssisRootDir";

        public Variable PackageRootVariable { get; private set; }

        public DTS.Package DtsPackage { get; private set; }

        public SsisProject SsisProject { get; private set; }

        public string PackageType { get; set; }

        public override Microsoft.SqlServer.Dts.Runtime.Executable DtsExecutable
        {
            get { return DtsPackage; }
        }

        public override Microsoft.SqlServer.Dts.Runtime.IDTSPropertiesProvider PropertyProvider
        {
            get { return DtsPackage; }
        }

        public override Microsoft.SqlServer.Dts.Runtime.IDTSSequence DtsSequence
        {
            get { return DtsPackage; }
        }

        public override Microsoft.SqlServer.Dts.Runtime.DtsContainer DtsContainer
        {
            get { return DtsPackage; }
        }

        public override Microsoft.SqlServer.Dts.Runtime.EventsProvider DtsEventsProvider
        {
            get { return DtsPackage; }
        }

        public string PackageFolderSubpath { get; set; }

        public string PackageFolder { get; set; }

        public string PackagePath { get; set; }

        public AST.Task.ProtectionLevel PackageProtectionLevel { get; set; }

        public string PackagePassword { get; set; }

        #region Private Storage
        private readonly DTS.Application _DTSApplication;
        #endregion  // Private Storage

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Required for Emitter pattern.")]
        public Package(AST.Task.AstPackageNode astNode)
            : base(astNode)
        {
            _DTSApplication = new DTS.Application();
            DtsPackage = new DTS.Package();
            DtsPackage.Name = StringManipulation.NameCleaner(Name);
            PackageType = String.IsNullOrEmpty(astNode.PackageType) ? "ETL" : astNode.PackageType;
            PackageFolder = astNode.PackageFolder;
            PackagePath = astNode.PackagePath;
            PackageProtectionLevel = astNode.ProtectionLevel;
            PackagePassword = astNode.PackagePassword;

            // vsabella: We thought about adding this in the Lowering phase.
            // The reason this was not placed in Lowering is that this variable must be available
            // before any other lowering can take place.  Additionally i needed a single place where the
            // variable name could remain constant and other lowering phase engines could refer to it.
            // PreEmit
            PackageRootVariable =
                new Variable(PackagePathRootVariableName)
                {
                    InheritFromPackageParentConfigurationString = "User::" + PackagePathRootVariableName,
                    ValueString = PathManager.TargetPath,
                    TypeCode = TypeCode.String
                };

            Children.Add(PackageRootVariable);
        }

        public override string Moniker
        {
            get { return null; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This needs to be a catch all for the package save method.")]
        private void Save()
        {
            try
            {
                _DTSApplication.UpdatePackage = true;
                _DTSApplication.UpdateObjects = true;

                Directory.CreateDirectory(PackageFolder);
                _DTSApplication.SaveToXml(PackagePath, DtsPackage, null);
                SsisProject.Save();
            }
            catch (Exception)
            {
                MessageEngine.Trace(AstNamedNode, Severity.Error, "V0101", "Failed to write package {0}. Files may be locked by another process.", Name);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need general exception handling.  No risk of bad state.")]
        public void ClearProjectDirectory()
        {
            try
            {
                DirectoryInfo packageDirectoryInfo = Directory.CreateDirectory(PackageFolder);
                if (packageDirectoryInfo.Exists)
                {
                    try
                    {
                        Directory.Delete(packageDirectoryInfo.FullName, true);
                    }
                    catch (IOException)
                    {
                    }

                    MessageEngine.Trace(Severity.Debug, "Deleted ETL Folder {0}", packageDirectoryInfo.FullName);
                }
            }
            catch (Exception e)
            {
                MessageEngine.Trace(Severity.Alert, "Error deleting ETL Folder {0}", e.Message);
            }
        }

        public override void Initialize(SsisEmitterContext context)
        {
            SsisProject = context.ProjectManager.AddPackage(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need general exception handling.  No risk of bad state.")]
        public override void Emit(SsisEmitterContext context)
        {
            MessageEngine.Trace(Severity.Notification, "Emitting Package {0}", PackagePath);
            DtsPackage.TransactionOption = (Microsoft.SqlServer.Dts.Runtime.DTSTransactionOption)
                Enum.Parse(typeof(Microsoft.SqlServer.Dts.Runtime.DTSTransactionOption), TransactionMode);
            DtsPackage.EnableConfigurations = true;

            switch (this.PackageProtectionLevel)
            {
                case AST.Task.ProtectionLevel.DontSaveSensitive:
                    DtsPackage.ProtectionLevel = DTSProtectionLevel.DontSaveSensitive;
                    break;
                case AST.Task.ProtectionLevel.EncryptAllWithPassword:
                    DtsPackage.ProtectionLevel = DTSProtectionLevel.EncryptAllWithPassword;
                    break;
                case AST.Task.ProtectionLevel.EncryptAllWithUserKey:
                    DtsPackage.ProtectionLevel = DTSProtectionLevel.EncryptAllWithUserKey;
                    break;
                case AST.Task.ProtectionLevel.EncryptSensitiveWithPassword:
                    DtsPackage.ProtectionLevel = DTSProtectionLevel.EncryptSensitiveWithPassword;
                    break;
                case AST.Task.ProtectionLevel.EncryptSensitiveWithUserKey:
                    DtsPackage.ProtectionLevel = DTSProtectionLevel.EncryptSensitiveWithUserKey;
                    break;
                case AST.Task.ProtectionLevel.ServerStorage:
                    DtsPackage.ProtectionLevel = DTSProtectionLevel.ServerStorage;
                    break;
                default:
                    DtsPackage.ProtectionLevel = DTSProtectionLevel.DontSaveSensitive;
                    break;
            }

            if (!string.IsNullOrEmpty(this.PackagePassword))
            {
                DtsPackage.PackagePassword = this.PackagePassword;
            }

            _DTSApplication.UpdatePackage = true;
            _DTSApplication.UpdateObjects = true;

            Directory.CreateDirectory(PackageFolder);

            context = new SsisEmitterContext(this, this, context.ProjectManager);

            PackageRootVariable.Initialize(context);
            PackageRootVariable.Emit(context);

            try
            {
                // Step #1 - i need to emit connection nodes and PackageConfig nodes first first
                var preEmittedNodeList = new List<PhysicalObject>();

                foreach (PhysicalObject po in this)
                {
                    if (po is Connections.Connection || po is PackageConfiguration)
                    {
                        po.Initialize(context);
                        po.Emit(context);
                        preEmittedNodeList.Add(po);
                    }
                }

                // Re-Initialize pre-emitted connection nodes and then remove
                // unused nodes so they no longer emit in the tree.
                foreach (var node in preEmittedNodeList)
                {
                    node.Parent = null;
                }

                base.Emit(context);
                Save();
            }
            catch (Exception e)
            {
                Save();
                MessageEngine.Trace(Severity.Error, e, e.Message);
            }
        }
    }
}
