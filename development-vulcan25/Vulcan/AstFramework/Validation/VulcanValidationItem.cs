using AstFramework.Model;

namespace AstFramework.Validation
{
    public class VulcanValidationItem
    {
        public IFrameworkItem InvalidItem { get; set; }

        public string PropertyName { get; set; }

        public string Message { get; set; }

        public string Recommendation { get; set; }

        public Severity Severity { get; set; }

        public ValidationCode ValidationCode { get; set; }

        public VulcanValidationItem()
        {
        }

        public VulcanValidationItem(IFrameworkItem invalidAstNode, string propertyName, Severity severity, ValidationCode validationCode, string message, string recommendation)
        {
            InvalidItem = invalidAstNode;
            PropertyName = propertyName;
            Severity = severity;
            ValidationCode = validationCode;
            Message = message;
            Recommendation = recommendation;
        }

        public override bool Equals(object obj)
        {
            var validationItem = obj as VulcanValidationItem;
            if (validationItem != null)
            {
                return Equals(validationItem);
            }

            return false;
        }

        public bool Equals(VulcanValidationItem validationItem)
        {
            bool propertyNameEquals = PropertyName == validationItem.PropertyName;
            bool severityEquals = Severity == validationItem.Severity;
            bool validationCodeEquals = ValidationCode == validationItem.ValidationCode;
            bool messageEquals = Message == validationItem.Message;
            bool recommendationEquals = Recommendation == validationItem.Recommendation;
            bool invalidItemEquals = InvalidItem == validationItem.InvalidItem;

            return propertyNameEquals && severityEquals && validationCodeEquals && messageEquals && recommendationEquals && invalidItemEquals;
        }

        public override int GetHashCode()
        {
            return InvalidItem.GetHashCode() +
                   PropertyName.GetHashCode() +
                   Severity.GetHashCode() +
                   ValidationCode.GetHashCode() +
                   Message.GetHashCode() +
                   Recommendation.GetHashCode();
        }
    }
}
