using Microsoft.InformationProtection;

namespace MIPSDK_WebApp1.Services
{
    public class ConsentDelegateImpl: IConsentDelegate
    {
        public Consent GetUserConsent(string url)
        {
            return Consent.Accept;
        }
    }
}
