using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Aardvark.Startup))]
namespace Aardvark
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
