namespace Tsinswreng.CsI18n;

using System.Globalization;
using Tsinswreng.CsCfg;

public class I18n:II18n{
	#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public I18n(ICfgAccessor CfgAccessor){
		this.CfgAccessor = CfgAccessor;
	}
	public I18n(){}
	public ICfgAccessor? CfgAccessor{get;set;}
	public OnKeyNotFound? OnKeyNotFound{get;set;} = (Self, Key, Args)=>{
		return Key.GetFullPathSegs().Last();
	};
	public str Get(II18nKey Key, params obj[] Args){
		if(CfgAccessor?.TryGet(Key.GetFullPathSegs(), out var Value) != true){
			if(OnKeyNotFound is null){
				return "";
			}
			return OnKeyNotFound(this, Key, Args);
		}
		if(Value is str Template){
			if(Args.Length == 0){
				return Template;
			}else{
				return FormatTemplate(Template, Args);
			}
		}
		//TODO handle Dict {type: "xxx", data: ""}
		throw new NotSupportedException();
	}

	/// <summary>
	/// 在 invariant globalization 模式下避免創建具體 CultureInfo。
	/// 語言資源目前使用的是標準 {0}/{1} 佔位符，因此直接走 composite formatting。
	/// </summary>
	protected virtual str FormatTemplate(str Template, obj[] Args){
		try{
			return string.Format(CultureInfo.InvariantCulture, Template, Args);
		}catch(FormatException){
			// 模板若寫錯，退回簡單替換，避免整個 UI 因提示文字崩潰。
			var ans = Template;
			for(var i = 0; i < Args.Length; i++){
				ans = ans.Replace("{"+i+"}", Args[i]?.ToString() ?? "");
			}
			return ans;
		}
	}

	public str this[II18nKey Key]{get{
		return Get(Key);
	}}
}

