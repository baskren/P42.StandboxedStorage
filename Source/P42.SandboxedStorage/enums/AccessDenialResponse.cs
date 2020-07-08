using System;
namespace P42.SandboxedStorage
{
    public enum AccessDenialResponse
    {
        Exception,
        //Alert,
        GlobalDefault,
        RequestAccess,
        Silent,
    }


    public static class AccessDenialResponseExtensions
    {
        public static void MakeGlobalDefault(this AccessDenialResponse accessDenialResponse)
        {
            if (accessDenialResponse != AccessDenialResponse.GlobalDefault)
                PlatformDelegate.DefaultAccessDenialResponse = accessDenialResponse;
        }

        public static AccessDenialResponse Value(this AccessDenialResponse accessDenialResponse)
        {
            if (accessDenialResponse == AccessDenialResponse.GlobalDefault)
                return PlatformDelegate.DefaultAccessDenialResponse;
            return accessDenialResponse;
        }

    }

}
