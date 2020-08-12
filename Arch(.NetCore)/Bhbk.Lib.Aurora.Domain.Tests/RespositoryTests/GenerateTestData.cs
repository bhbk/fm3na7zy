using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure;
using Bhbk.Lib.Aurora.Data.Models;
using Bhbk.Lib.Aurora.Primitives;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Linq;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Hashing;

namespace Bhbk.Lib.Aurora.Domain.Tests.RespositoryTests
{
    public class GenerateTestData
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GenerateTestData(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        public void Create()
        {

        }

        public void Destroy()
        {
            /*
             * delete default users
             */

            _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<uvw_Users>()
                .Where(x => x.UserName == Constants.TestCompositeUser).ToLambda());

            _uow.Commit();

        }
    }
}
