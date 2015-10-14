using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CharecterSheet
{
    public class CustomModelBinder : DefaultModelBinder
    {
        protected override object GetPropertyValue(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, IModelBinder propertyBinder)
        {
            var propertyType = propertyDescriptor.PropertyType;

            // Check if the property type is an enum with the flag attribute
            if (propertyType.IsEnum && propertyType.GetCustomAttributes(true).Any())
            {
                var providerValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (providerValue != null)
                {
                    var value = providerValue.RawValue;
                    if (value != null)
                    {
                        // In case it is a checkbox list/dropdownlist/radio button list
                        if (value is string[])
                        {
                            // Create flag value from posted values
                            int flagValue = 0;
                            foreach (string val in ((string[])value))
                            {
                                flagValue = flagValue | (int)Enum.Parse(propertyType, val);
                            }
                            return Enum.ToObject(propertyType, flagValue);
                        }
                        // In case it is a single value
                        if (value.GetType().IsEnum)
                        {
                            return Enum.ToObject(propertyType, value);
                        }
                    }
                }
            }
            return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
        }
    }

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            
            ModelBinders.Binders.DefaultBinder = new CustomModelBinder();
        }
    }
}
