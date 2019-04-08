using Followme.AspNet.Core.FastCommon.Infrastructure;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Followme.AspNet.Core.FastCommon.ThirdParty.GrpcServer
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GrpcMessageResult : MessageResult
    {
        public GrpcMessageResult()
        {
        }

        public GrpcMessageResult(ResultCode code):base(code,"")
        {}

        public GrpcMessageResult(ResultCode code, string message) : base(code, message)
        {
        }

        public override ResultCode Code { get; set; }

        public override string Message { get; set; }

        public override int Status { get => base.Status; set => base.Status = value; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GrpcSuccess : GrpcMessageResult
    {
        public GrpcSuccess(string message="OK"):base(ResultCode.Success,message)
        {
        }

        public override ResultCode Code { get; set; }

        public override string Message { get; set; }

        public override int Status { get => base.Status; set => base.Status = value; }
    }

}
