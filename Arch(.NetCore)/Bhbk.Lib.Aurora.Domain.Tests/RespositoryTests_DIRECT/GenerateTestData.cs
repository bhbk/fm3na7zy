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
            var dsaKey = _uow.SysPrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SysPrivateKeys>()
                .Where(x => x.KeyValueAlgo == SshHostKeyAlgorithm.DSS.ToString()).ToLambda())
                .OrderBy(x => x.Created).LastOrDefault();

            if (dsaKey == null)
            {
                dsaKey = KeyHelper.GenerateSshPrivateKey(_uow,
                    SshHostKeyAlgorithm.DSS, 1024, AlphaNumeric.CreateString(32), SshPrivateKeyFormat.Pkcs8);

                _uow.Commit();
            }

            var rsaKey = _uow.SysPrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SysPrivateKeys>()
                .Where(x => x.KeyValueAlgo == SshHostKeyAlgorithm.RSA.ToString()).ToLambda())
                .OrderBy(x => x.Created).LastOrDefault();

            if (rsaKey == null)
            {
                rsaKey = KeyHelper.GenerateSshPrivateKey(_uow,
                    SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32), SshPrivateKeyFormat.Pkcs8);

                _uow.Commit();
            }

            var ecdsaKey = _uow.SysPrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SysPrivateKeys>()
                .Where(x => x.KeyValueAlgo == SshHostKeyAlgorithm.ECDsaNistP521.ToString()).ToLambda())
                .OrderBy(x => x.Created).LastOrDefault();

            if (ecdsaKey == null)
            {
                ecdsaKey = KeyHelper.GenerateSshPrivateKey(_uow,
                    SshHostKeyAlgorithm.ECDsaNistP521, 521, AlphaNumeric.CreateString(32), SshPrivateKeyFormat.Pkcs8);

                _uow.Commit();
            }

            var ed25519Key = _uow.SysPrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SysPrivateKeys>()
                .Where(x => x.KeyValueAlgo == SshHostKeyAlgorithm.ED25519.ToString()).ToLambda())
                .OrderBy(x => x.Created).LastOrDefault();

            if (ed25519Key == null)
            {
                ed25519Key = KeyHelper.GenerateSshPrivateKey(_uow,
                    SshHostKeyAlgorithm.ED25519, 256, AlphaNumeric.CreateString(32), SshPrivateKeyFormat.Pkcs8);

                _uow.Commit();
            }

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
                        FileSystemType = FileSystemTypes.Composite.ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                _uow.UserPasswords.Create(
                    new tbl_UserPasswords
                    {
                        UserId = foundCompositeUser.Id,
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        HashPBKDF2 = PBKDF2.Create(Constants.TestCompositeUserPass),
                        HashSHA256 = SHA256.Create(Constants.TestCompositeUserPass),
                        SecurityStamp = Guid.NewGuid().ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                var key = KeyHelper.GenerateSshPrivateKey(_uow, foundCompositeUser, 
                    SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256, Dns.GetHostName());

                _uow.Commit();
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
                        FileSystemType = FileSystemTypes.Memory.ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                _uow.UserPasswords.Create(
                    new tbl_UserPasswords
                    {
                        UserId = foundMemoryUser.Id,
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        HashPBKDF2 = PBKDF2.Create(Constants.TestMemoryUserPass),
                        HashSHA256 = SHA256.Create(Constants.TestMemoryUserPass),
                        SecurityStamp = Guid.NewGuid().ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                var key = KeyHelper.GenerateSshPrivateKey(_uow, foundMemoryUser,
                    SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256, Dns.GetHostName());

                _uow.Commit();
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
                        FileSystemType = FileSystemTypes.SMB.ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                _uow.UserPasswords.Create(
                    new tbl_UserPasswords
                    {
                        UserId = foundSmbUser.Id,
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        HashPBKDF2 = PBKDF2.Create(Constants.TestSmbUserPass),
                        HashSHA256 = SHA256.Create(Constants.TestSmbUserPass),
                        SecurityStamp = Guid.NewGuid().ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                var key = KeyHelper.GenerateSshPrivateKey(_uow, foundSmbUser,
                    SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256, Dns.GetHostName());

                _uow.Commit();
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
            _uow.SysPrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_SysPrivateKeys>()
                .Where(x => x.KeyValueAlgo == SshHostKeyAlgorithm.DSS.ToString()
                    || x.KeyValueAlgo == SshHostKeyAlgorithm.RSA.ToString()
                    || x.KeyValueAlgo == SshHostKeyAlgorithm.ECDsaNistP521.ToString()
                    || x.KeyValueAlgo == SshHostKeyAlgorithm.ED25519.ToString()).ToLambda());

            _uow.Commit();
        }
    }
}
