using Comments.Application.Abstractions;
using Comments.Infrastructure.Persistence;
using Comments.Infrastructure.Repositories;
using Comments.Infrastructure.Services;
using Lazy.Captcha.Core;
using Lazy.Captcha.Core.Generator;
using Lazy.Captcha.Core.Generator.Code;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Comments.Infrastructure;

/// <summary>Registers the Infrastructure layer (EF Core, repositories) into DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

        services.AddDbContext<CommentsDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddCaptcha(options =>
        {
            options.CaptchaType = CaptchaType.DEFAULT; // Latin letters + digits (per the spec)
            options.CodeLength = 5;
            options.ExpirySeconds = 300;               // valid for 5 minutes
        });

        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICaptchaService, LazyCaptchaService>();

        return services;
    }
}
