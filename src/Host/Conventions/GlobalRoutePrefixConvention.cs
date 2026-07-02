using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace KaraWeb.Server.Host.Conventions
{
    public class GlobalRoutePrefixConvention : IControllerModelConvention
    {
        private readonly AttributeRouteModel _centralPrefix;

        public GlobalRoutePrefixConvention(string prefix)
        {
            _centralPrefix = new AttributeRouteModel(new RouteAttribute(prefix));
        }

        public void Apply(ControllerModel controller)
        {
            if (!controller.Selectors.Any())
            {
                return;
            }

            foreach (var selectorModel in controller.Selectors)
            {
                selectorModel.AttributeRouteModel =
                    AttributeRouteModel.CombineAttributeRouteModel(_centralPrefix, selectorModel.AttributeRouteModel);
            }
        }
    }
}