namespace Atlantis.Grpc.Utilies
{
    public interface IMessageResult
    {
        string Message{get;}

        ResultCode Code{get;set;}

        int Status{get;set;}
    }

    public class MessageResult:IMessageResult
    {
        public MessageResult()
        {
            Message = "";
        }

        public MessageResult(ResultCode code,string message)
        {
            Code = code;
            Message = message;
        }

        public virtual string Message { get; set; }

        public virtual ResultCode Code { get; set; }

        public virtual int Status { get=>Code.ToStatus();set=>value.ToString(); }

        public bool IsSucceed()=> Code == ResultCode.Success;
    }

    public class PageMessageResult : MessageResult
    {
        public PageMessageResult()
        {
        }

        public PageMessageResult(ResultCode code, string message) : base(code, message)
        {
        }

        public PageMessageResult(int pageIndex,int pageSize,int total,int totalPage) : base(ResultCode.Success, "")
        {
            PageIndex=pageIndex;
            PageSize=pageSize;
            TotalPage=totalPage;
            Total=total;
        }

        public virtual int PageIndex { get; set; }

        public virtual int PageSize { get; set; }

        public virtual int Total { get; set; }

        public virtual int TotalPage { get; set; }
    }

    public class MessageResult<T>:MessageResult where T:class
    {
        public MessageResult(ResultCode code,string msg):base(code,msg)
        {}

        public MessageResult(T data,ResultCode code=ResultCode.Success,string msg="OK"):base(code,msg)
        {
            Data=data;
        }

        public T Data { get; set; }
    }

    public enum ResultCode
    {
        Success=0,
        BussinessError=-1,
        Exception=-2
    }
}
