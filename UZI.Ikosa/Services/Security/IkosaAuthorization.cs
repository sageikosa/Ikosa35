using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;
using System.Security.Principal;
using System.ServiceModel.Description;
using System.ServiceModel;

namespace Uzi.Ikosa.Services
{
    public class IkosaAuthorization : IAuthorizationPolicy
    {
        #region IAuthorizationPolicy Members

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            // authorize Metadata requests
            if (OperationContext.Current.EndpointDispatcher.ContractName == ServiceMetadataBehavior.MexContractName &&
                OperationContext.Current.EndpointDispatcher.ContractNamespace == @"http://schemas.microsoft.com/2006/04/mex" &&
                OperationContext.Current.IncomingMessageHeaders.Action == @"http://schemas.xmlsoap.org/ws/2004/09/transfer/Get")
            {
                evaluationContext.Properties[@"Principal"] = new GenericPrincipal(new GenericIdentity("MexAccount"), null);
                return true;
            }

            if (!evaluationContext.Properties.TryGetValue(@"Identities", out var _idProperty))
            {
                return false;
            }

            // ensure we have at least one IIdentity
            if ((_idProperty is IList<IIdentity> _identities)
                && (_identities.Count > 0))
            {
                // get principal and attach to context
                var _identity = _identities[0];
                var _principal = new IkosaPrincipal(_identity, UserValidator.UserDefinitions.GetUser(_identity.Name));
                evaluationContext.Properties[@"Principal"] = _principal;
                return true;
            }
            else
            {
                return false;
            }
        }

        public ClaimSet Issuer { get { return ClaimSet.System; } }

        #endregion

        #region IAuthorizationComponent Members
        private string _ID = Guid.NewGuid().ToString();
        public string Id
        {
            get { return _ID; }
        }
        #endregion
    }
}