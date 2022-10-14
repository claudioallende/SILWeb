using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.AtributosValidacion
{
    public class NotEqualAttribute : ValidationAttribute
    {
        private string OtherProperty { get; set; }
        private string PropertyName { get; set; }
        private string OtherPropertyName { get; set; }

        public NotEqualAttribute(string otherProperty, string propertyName = "", string otherPropertyName = "")
        {
            OtherProperty = otherProperty;
            PropertyName = propertyName;
            OtherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // get other property value
            var otherPropertyInfo = validationContext.ObjectType.GetProperty(OtherProperty);
            var otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance);

            // verify values
            if (value != null && otherValue != null)
            {
                if (value.ToString().Equals(otherValue.ToString()))
                    return new ValidationResult(string.Format("{0} no puede ser igual que {1}.", PropertyName == "" ? validationContext.MemberName : PropertyName, OtherPropertyName == "" ? OtherProperty : OtherPropertyName));
                else
                    return ValidationResult.Success;
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
}