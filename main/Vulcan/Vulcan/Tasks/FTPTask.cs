/*
 * Microsoft Public License (Ms-PL)
 * 
 * This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 * 
 * 1. Definitions
 * The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 * A "contribution" is the original software, or any additions or changes to the software.
 * A "contributor" is any person that distributes its contribution under this license.
 * "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 * 
 * 2. Grant of Rights
 * (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 * (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 * 
 * 3. Conditions and Limitations
 * (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 * (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 * (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 * (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 * (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;


using Vulcan.Common;
using Vulcan.Common.Helpers;
using Vulcan.Tasks;
using Vulcan.Common.Templates;

using Vulcan.Emitters;
using Vulcan.Properties;

using DTS = Microsoft.SqlServer.Dts.Runtime;
using DTSTasks = Microsoft.SqlServer.Dts.Tasks;


namespace Vulcan.Tasks
{
    public class FTPTask : Task
    {
        public DTS.TaskHost fTPTask;

        public FTPTask(
            Packages.VulcanPackage vulcanPackage,
            string name,
            string taskDescription,
            DTS.IDTSSequence parentContainer,
            string serverName,
            string port,
            string userName,
            string password,
            string remotePath,
            bool isRemotePathVariable,
            string localPath,
            bool isLocalPathVariable,
            Microsoft.SqlServer.Dts.Tasks.FtpTask.DTSFTPOp operation
            )
            :
            base(
            vulcanPackage,
            name,
            taskDescription,
            parentContainer
            )
        {
            fTPTask = (DTS.TaskHost)parentContainer.Executables.Add("STOCK:FTPTask");
            fTPTask.Name = name;
            fTPTask.Description = taskDescription;
            this.Task.Operation = operation;

            this.Task.IsRemotePathVariable = isRemotePathVariable;
            this.Task.IsLocalPathVariable = isLocalPathVariable;

            Connection fTPServerConnection = new Connection(vulcanPackage, serverName, serverName, "FTP", String.Format("\"{0}:{1}\"", serverName, port));
            fTPServerConnection.SetProperty("ServerUserName", userName);
            fTPServerConnection.SetProperty("ServerPassword", password);
            this.Task.Connection = fTPServerConnection.ConnectionManager.ID;

            if (this.Task.IsRemotePathVariable == true)
            {
                this.Task.RemotePath = remotePath;
            }
            else
            {
                this.Task.RemotePath = remotePath;
            }

            if (this.Task.IsLocalPathVariable == true)
            {
                this.Task.LocalPath = localPath;
            }
            else
            {
                Connection localConnection = new Connection(vulcanPackage, localPath, localPath, "File", String.Format("\"{0}\"", ExpressionPathBuilder.EscapeBackslashes(localPath)));
                int intFileUsageType = 2;
                switch (operation)
                {
                    case Microsoft.SqlServer.Dts.Tasks.FtpTask.DTSFTPOp.Send:
                        intFileUsageType = 0;
                        break;
                    case Microsoft.SqlServer.Dts.Tasks.FtpTask.DTSFTPOp.Receive:
                        intFileUsageType = 2;
                        break;
                    default:
                        intFileUsageType = 2;
                        break;
                }
                localConnection.SetProperty("FileUsageType", intFileUsageType);

                this.Task.LocalPath = localPath;
            }
        }

        public override void SetProperty(string name, object value)
        {
            fTPTask.Properties[name].SetValue(fTPTask, value);
        }

        public override void SetExpression(string expressionName, string expressionValue)
        {
            fTPTask.SetExpression(expressionName, expressionValue);
        }

        public DTS.TaskHost TaskHost
        {
            get
            {
                return this.fTPTask;
            }
        }

        public DTSTasks.FtpTask.FtpTask Task
        {
            get
            {
                return (DTSTasks.FtpTask.FtpTask)this.fTPTask.InnerObject;
            }
        }
    }
}
