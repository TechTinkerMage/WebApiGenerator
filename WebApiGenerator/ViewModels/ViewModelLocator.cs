using WebApiGenerator.Interfaces;
using WebApiGenerator.IoC;

namespace WebApiGenerator.ViewModels
{
    public class ViewModelLocator
    {
        public static MainViewModel MainViewModel => DIContainer.Resolve<MainViewModel>();
    }
}
