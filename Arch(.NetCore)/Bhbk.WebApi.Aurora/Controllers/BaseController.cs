﻿using AutoMapper;
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
        protected IMapper Mapper { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IMapper>(); }
        protected IUnitOfWork UoW { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); }
        protected IConfiguration Conf { get => ControllerContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>(); }
    }
}
