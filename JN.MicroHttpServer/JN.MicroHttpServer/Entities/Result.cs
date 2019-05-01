namespace JN.MicroHttpServer.Entities
{
    public class Result
    {
        public bool Success;
        public int ErrorCode;
        public string ErrorDescription;
        public string JsonContent;
        public bool Authenticated = true;
    }
}
