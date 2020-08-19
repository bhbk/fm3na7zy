using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Linq;

namespace Bhbk.Lib.Aurora.Domain.Infrastructure
{
    public class GenerateDefaultData
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GenerateDefaultData(IUnitOfWork uow, IMapper mapper)
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
            _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.IdentityAlias == Constants.DefaultUser).ToLambda());

            _uow.Commit();

        }
    }
}
