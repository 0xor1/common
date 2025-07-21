using System.ComponentModel;
using System.Runtime.Serialization;

namespace Common.Server;

public enum Env
{
    [EnumMember(Value = "lcl")]
    [Description("lcl")]
    Lcl,

    [EnumMember(Value = "dev")]
    [Description("dev")]
    Dev,

    [EnumMember(Value = "stg")]
    [Description("stg")]
    Stg,

    [EnumMember(Value = "pro")]
    [Description("pro")]
    Pro,
}
