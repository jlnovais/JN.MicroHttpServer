namespace JN.MicroHttpServer.Dto
{
    public class Result
    {
        public bool Success;
        public int ErrorCode;
        public string ErrorDescription;
        public string Content;
        public bool Authenticated = true;
    }
}
