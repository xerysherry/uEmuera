using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.Sub;

namespace MinorShift.Emuera.GameProc.Function
{

	internal abstract class AbstractInstruction
	{
	    protected int flag;
	    public int Flag{ get{return flag;} }
	    
	    public ArgumentBuilder ArgBuilder {get; protected set;}
	    public virtual void SetJumpTo(ref bool useCallForm, InstructionLine func, int currentDepth, ref string FunctionoNotFoundName) { }
	    public virtual void DoInstruction(ExpressionMediator exm, InstructionLine func, ProcessState state)
		{ throw new ExeEE("未実装 or 呼び出しミス"); }
		
		public virtual Argument CreateArgument(InstructionLine line, ExpressionMediator exm)
		{
			throw new ExeEE("実装されていない");
		}
			
	}
	
}
