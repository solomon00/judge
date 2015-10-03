﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solomon.TypesExtensions.ValidationAttributeExtensions
{
    public enum Comparison
    {
        IsEqualTo,
        IsNotEqualTo
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredIfAttribute : ValidationAttribute, IClientValidatable
    {
        private const string DefaultErrorMessageFormatString = "Обязательное поле";

        public string OtherProperty { get; private set; }
        public Comparison Comparison { get; private set; }
        public object Value { get; private set; }

        public RequiredIfAttribute(string otherProperty, Comparison comparison, object value)
        {
            if (string.IsNullOrEmpty(otherProperty))
            {
                throw new ArgumentNullException("otherProperty");
            }

            OtherProperty = otherProperty;
            Comparison = comparison;
            Value = value;

            ErrorMessage = DefaultErrorMessageFormatString;
        }

        private bool Validate(object actualPropertyValue)
        {
            switch (Comparison)
            {
                case Comparison.IsNotEqualTo:
                    return actualPropertyValue == null || !actualPropertyValue.Equals(Value);
                default:
                    return actualPropertyValue != null && actualPropertyValue.Equals(Value);
            }
        }

        protected override ValidationResult IsValid(object value,
                                                    ValidationContext validationContext)
        {
            if (value == null)
            {
                var property = validationContext.ObjectInstance.GetType()
                                                .GetProperty(OtherProperty);

                var propertyValue = property.GetValue(validationContext.ObjectInstance, null);

                if (Validate(propertyValue))
                {
                    return new ValidationResult(
                        string.Format(ErrorMessageString, validationContext.DisplayName));
                }
            }
            return ValidationResult.Success;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(
                                                            ModelMetadata metadata,
                                                            ControllerContext context)
        {
            return new[]
        {
            new ModelClientValidationRequiredIfRule(string.Format(ErrorMessageString, 
                metadata.GetDisplayName()), OtherProperty, Comparison, Value)
        };
        }
    }

    public class ModelClientValidationRequiredIfRule : ModelClientValidationRule
    {
        public ModelClientValidationRequiredIfRule(string errorMessage,
                                                   string otherProperty,
                                                   Comparison comparison,
                                                   object value)
        {
            ErrorMessage = errorMessage;
            ValidationType = "requiredif";
            ValidationParameters.Add("other", otherProperty);
            ValidationParameters.Add("comp", comparison.ToString().ToLower());
            ValidationParameters.Add("value", value.ToString().ToLower());
        }
    }
}
