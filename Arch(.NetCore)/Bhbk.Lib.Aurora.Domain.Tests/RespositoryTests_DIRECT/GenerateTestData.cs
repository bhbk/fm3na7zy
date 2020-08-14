using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Primitives;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Rebex.Net;
using Rebex.Security.Certificates;
using System;
using System.Linq;
using System.Net;

namespace Bhbk.Lib.Aurora.Domain.Tests.RespositoryTests_DIRECT
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
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * create key pairs for daemons
             */
            KeyHelper.CheckDaemonSshPrivKey(_uow, SshHostKeyAlgorithm.DSS, 1024, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
            KeyHelper.CheckDaemonSshPrivKey(_uow, SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
            KeyHelper.CheckDaemonSshPrivKey(_uow, SshHostKeyAlgorithm.ECDsaNistP521, 521, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
            KeyHelper.CheckDaemonSshPrivKey(_uow, SshHostKeyAlgorithm.ED25519, 256, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);

            /*
             * create composite test users
             */
            var foundCompositeUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                .Where(x => x.UserName == Constants.TestCompositeUser).ToLambda())
                .SingleOrDefault();

            if (foundCompositeUser == null)
            {
                foundCompositeUser = _uow.Users.Create(
                    new tbl_Users()
                    {
                        Id = Guid.NewGuid(),
                        UserName = Constants.TestCompositeUser,
                        AllowPassword = true,
                        FileSystemType = FileSystemTypes.Composite.ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                _uow.Commit();

                KeyHelper.CreateUserSshPrivKey(_uow, foundCompositeUser,
                    SshPrivateKeyFormat.Pkcs8, SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32),
                    SshPublicKeyFormat.Pkcs8, SignatureHashAlgorithm.SHA256, Dns.GetHostName());
            }

            /*
             * create memory test users
             */
            var foundMemoryUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                .Where(x => x.UserName == Constants.TestMemoryUser).ToLambda())
                .SingleOrDefault();

            if (foundMemoryUser == null)
            {
                foundMemoryUser = _uow.Users.Create(
                    new tbl_Users()
                    {
                        Id = Guid.NewGuid(),
                        UserName = Constants.TestMemoryUser,
                        AllowPassword = true,
                        FileSystemType = FileSystemTypes.Memory.ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                _uow.Commit();

                KeyHelper.CreateUserSshPrivKey(_uow, foundMemoryUser,
                    SshPrivateKeyFormat.Pkcs8, SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32),
                    SshPublicKeyFormat.Pkcs8, SignatureHashAlgorithm.SHA256, Dns.GetHostName());
            }

            /*
             * create smb test users
             */
            var foundSmbUser = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                .Where(x => x.UserName == Constants.TestSmbUser).ToLambda())
                .SingleOrDefault();

            if (foundSmbUser == null)
            {
                foundSmbUser = _uow.Users.Create(
                    new tbl_Users()
                    {
                        Id = Guid.NewGuid(),
                        UserName = Constants.TestSmbUser,
                        AllowPassword = true,
                        FileSystemType = FileSystemTypes.SMB.ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                _uow.Commit();

                KeyHelper.CreateUserSshPrivKey(_uow, foundSmbUser,
                    SshPrivateKeyFormat.Pkcs8, SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32),
                    SshPublicKeyFormat.Pkcs8, SignatureHashAlgorithm.SHA256, Dns.GetHostName());
            }
        }

        public void Destroy()
        {
            if (_uow.InstanceType == InstanceContext.DeployedOrLocal)
                throw new InvalidOperationException();

            /*
             * delete test users
             */
            _uow.Users.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                .Where(x => x.UserName == Constants.TestCompositeUser 
                    || x.UserName == Constants.TestMemoryUser
                    || x.UserName == Constants.TestSmbUser).ToLambda());

            _uow.Commit();

            /*
             * delete key pairs for daemons
             */
            _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
                .Where(x => (x.KeyAlgo == SshHostKeyAlgorithm.DSS.ToString()
                    || x.KeyAlgo == SshHostKeyAlgorithm.RSA.ToString()
                    || x.KeyAlgo == SshHostKeyAlgorithm.ECDsaNistP521.ToString()
                    || x.KeyAlgo == SshHostKeyAlgorithm.ED25519.ToString()) && x.UserId == null).ToLambda());

            _uow.Commit();
        }
    }
}
