using EmiratesKit.Core.Interfaces;
using EmiratesKit.Core.Validators;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiratesKit.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUaeValidators(
            this IServiceCollection services)
        {
            services.AddSingleton<IEmiratesIdValidator, EmiratesIdValidator>();
            services.AddSingleton<IUaeIbanValidator, UaeIbanValidator>();
            services.AddSingleton<UaeTrnValidator>();
            services.AddSingleton<UaeMobileValidator>();
            services.AddSingleton<UaePassportValidator>();
            return services;
        }
    }
}
