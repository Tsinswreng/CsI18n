namespace Tsinswreng.CsI18n;

public interface ILang:IDictionary<obj, obj?>{
	[Doc("語言標識代碼 如zh-Hant")]
	public str Code{get;set;}
	[Doc("類型 如 Bcp47。空即默認。")]
	public str? Type{get;set;}
}

public class Lang
	:Dictionary<obj, obj?>
	,ILang
{
	public Lang(){
		
	}
	public Lang(str Code){
		this.Code = Code;
	}
	public Lang(str Code, str? Type){
		this.Code = Code;
		this.Type = Type;
	}
	public str Code{get;set;} = "";
	public str? Type{get;set;}
}
