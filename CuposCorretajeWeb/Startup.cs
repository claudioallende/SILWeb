using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CuposCorretajeWeb.Startup))]
namespace CuposCorretajeWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
