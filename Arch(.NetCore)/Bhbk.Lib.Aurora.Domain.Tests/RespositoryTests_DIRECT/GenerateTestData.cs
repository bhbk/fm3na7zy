using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Primitives;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Rebex.Net;
using Rebex.Security.Certificates;
using System;
using System.Linq;
using System.Net;

namespace Bhbk.Lib.Aurora.Domain.Tests.RespositoryTests_DIRECT
{
    public class GenerateTestData
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GenerateTestData(IConfiguration conf, IUnitOfWork uow, IMapper mapper)
        {
            _conf = conf ?? throw new ArgumentNullException();
            _uow = uow ?? throw new ArgumentNullException();
            _mapper = mapper ?? throw new ArgumentNullException();
        }

        public void Create()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create key pairs for daemons
             */
            KeyHelper.CheckPrivKey(_conf, _uow, SshHostKeyAlgorithm.DSS, 1024, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
            KeyHelper.CheckPrivKey(_conf, _uow, SshHostKeyAlgorithm.RSA, 4096, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
            KeyHelper.CheckPrivKey(_conf, _uow, SshHostKeyAlgorithm.ECDsaNistP256, 256, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
            KeyHelper.CheckPrivKey(_conf, _uow, SshHostKeyAlgorithm.ECDsaNistP384, 384, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
            KeyHelper.CheckPrivKey(_conf, _uow, SshHostKeyAlgorithm.ECDsaNistP521, 521, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
            KeyHelper.CheckPrivKey(_conf, _uow, SshHostKeyAlgorithm.ED25519, 256, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);

            /*
             * create composite test users
             */
            var foundCompositeUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.IdentityAlias == Constants.TestCompositeUser).ToLambda())
                .SingleOrDefault();

            if (foundCompositeUser == null)
            {
                foundCompositeUser = _uow.Users.Create(
                    new tbl_User()
                    {
                        IdentityId = Guid.NewGuid(),
                        IdentityAlias = Constants.TestCompositeUser,
                        RequirePassword = true,
                        RequirePublicKey = false,
                        FileSystemType = FileSystemTypes.Composite.ToString(),
                        CreatedUtc = DateTime.UtcNow,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                KeyHelper.CreatePrivKey(_conf, _uow, foundCompositeUser, SshHostKeyAlgorithm.RSA, 2048, 
                    AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256, Dns.GetHostName());
            }

            /*
             * create memory test users
             */
            var foundMemoryUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.IdentityAlias == Constants.TestMemoryUser).ToLambda())
                .SingleOrDefault();

            if (foundMemoryUser == null)
            {
                foundMemoryUser = _uow.Users.Create(
                    new tbl_User()
                    {
                        IdentityId = Guid.NewGuid(),
                        IdentityAlias = Constants.TestMemoryUser,
                        RequirePassword = true,
                        RequirePublicKey = false,
                        FileSystemType = FileSystemTypes.Memory.ToString(),
                        CreatedUtc = DateTime.UtcNow,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                KeyHelper.CreatePrivKey(_conf, _uow, foundMemoryUser, SshHostKeyAlgorithm.RSA, 2048, 
                    AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256, Dns.GetHostName());
            }

            /*
             * create smb test users
             */
            var foundSmbUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.IdentityAlias == Constants.TestSmbUser).ToLambda())
                .SingleOrDefault();

            if (foundSmbUser == null)
            {
                foundSmbUser = _uow.Users.Create(
                    new tbl_User()
                    {
                        IdentityId = Guid.NewGuid(),
                        IdentityAlias = Constants.TestSmbUser,
                        RequirePassword = true,
                        RequirePublicKey = false,
                        FileSystemType = FileSystemTypes.SMB.ToString(),
                        CreatedUtc = DateTime.UtcNow,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                KeyHelper.CreatePrivKey(_conf, _uow, foundSmbUser, SshHostKeyAlgorithm.RSA, 2048, 
                    AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256, Dns.GetHostName());
            }
        }

        public void Destroy()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * delete test users
             */
            _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                .Where(x => x.IdentityAlias == Constants.TestCompositeUser 
                    || x.IdentityAlias == Constants.TestMemoryUser
                    || x.IdentityAlias == Constants.TestSmbUser).ToLambda());

            _uow.Commit();

            /*
             * delete key pairs for daemons
             */
            _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKey>()
                .Where(x => (x.KeyAlgo == SshHostKeyAlgorithm.DSS.ToString()
                    || x.KeyAlgo == SshHostKeyAlgorithm.RSA.ToString()
                    || x.KeyAlgo == SshHostKeyAlgorithm.ECDsaNistP521.ToString()
                    || x.KeyAlgo == SshHostKeyAlgorithm.ED25519.ToString()) && x.IdentityId == null).ToLambda());

            _uow.Commit();
        }
    }
}
