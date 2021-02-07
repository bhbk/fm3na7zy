using AutoMapper;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bhbk.WebApi.Aurora.Controllers
{
    [Authorize(Roles = DefaultConstants.RoleForViewers_Aurora + ", " 
        + DefaultConstants.RoleForUsers_Aurora + ", " 
        + DefaultConstants.RoleForAdmins_Aurora)]
    public class BaseController : Controller
    {
        protected IMapper map { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IMapper>(); }
        protected IUnitOfWork uow { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); }
        protected IConfiguration conf { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>(); }
    }
}
