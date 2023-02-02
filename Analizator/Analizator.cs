using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Analizators
{
	
	public enum LexemeType {For, To, Next, Not, Relation, ArithmeticOperation, Assignment, IdOrConst }

	public enum LexemeClass { Keyword, Identifier, Constant, SpecialSymbols, IdOrConst }

	public enum State { Start, Identifier, Constant, Error, Final, Comparison, ReverseComparison, ArithmeticOperation, Assignment }

	public class Analizator
	{
        private int indexLexems;
        private int posForLoopCondition;
		private List<string> counterValue;
        private Stack<string> stackInterpret;


        public Dictionary<string, int> varValues;
		public List<Lexeme> Lexemes { get; private set; }
		public List<string> poliz { get; private set; }

		public Analizator()
		{
            indexLexems = 0;
            posForLoopCondition = 0;
			counterValue = new List<string>();
            stackInterpret = new Stack<string>();

            Lexemes = new List<Lexeme>();
			poliz = new List<string>();
			varValues = new Dictionary<string, int>();
        }
		
		public bool LexicalAnalysis(string text)
		{
			Lexemes = new List<Lexeme>();
            State state = State.Start;
            State prevState;
			bool AddLexemeFlag;
			text += " ";
			StringBuilder next = new StringBuilder();
			StringBuilder current = new StringBuilder();
			int textIndex = 0;
			while (state != State.Error && state != State.Final)
			{
				prevState = state;
				AddLexemeFlag = true;
				if (textIndex == text.Length && state != State.Error)
				{
					state = State.Final;
					break;
				}
				if (textIndex == text.Length)
				{
					break;
				}
				char symbol = text[textIndex];
				switch (state)
				{
					case State.Start:
						if (char.IsWhiteSpace(symbol)) state = State.Start;
						else if (char.IsDigit(symbol)) state = State.Constant;
						else if (char.IsLetter(symbol)) state = State.Identifier;
						else if (symbol == '>') state = State.Comparison;
						else if (symbol == '<') state = State.ReverseComparison;
						else if (symbol == '+' || symbol == '-' || symbol == '/' || symbol == '*') state = State.ArithmeticOperation;
						else if (symbol == '=') state = State.Assignment;
						else state = State.Error;
						AddLexemeFlag = false;
						if (!char.IsWhiteSpace(symbol))
							current.Append(symbol);
						break;
					case State.Comparison:
						if (char.IsWhiteSpace(symbol))
						{
							state = State.Start;
						}
						else if (char.IsLetter(symbol))
						{
							state = State.Identifier;
							next.Append(symbol);
						}
						else if (char.IsDigit(symbol))
						{
							state = State.Constant;
							next.Append(symbol);
						}
						else
						{
							state = State.Error;
							AddLexemeFlag = false;
						}

						break;
					case State.ReverseComparison:
						if (char.IsWhiteSpace(symbol)) state = State.Start;
						else if (symbol == '>')
						{
							state = State.Start;
							current.Append(symbol);
						}
						else if (symbol == '=')
						{
							state = State.Start;
							current.Append(symbol);
						}
						else if (char.IsLetter(symbol))
						{
							state = State.Identifier;
							next.Append(symbol);
						}
						else if (char.IsDigit(symbol))
						{
							state = State.Constant;
							next.Append(symbol);
						}
						else
						{
							state = State.Error;
							AddLexemeFlag = false;
						}
						break;
					case State.Assignment:
						if (symbol == '=' && text[textIndex-1] == '=')
						{
                            state = State.Start;
							current.Append(symbol);
						}
                        else if (symbol == '=')
                        {
                            state = State.ReverseComparison;
                            current.Append(symbol);
                        }
						else if (char.IsWhiteSpace(symbol))
						{
							state = State.Start;
						}
						else
						{
							state = State.Error;
							AddLexemeFlag = false;
						}
						break;
					case State.Constant:
						if (char.IsWhiteSpace(symbol)) state = State.Start;
						else if (char.IsDigit(symbol))
						{
							state = State.Constant;
							current.Append(symbol);
						}
						else if (symbol == '<')
						{
							state = State.ReverseComparison;
							next.Append(symbol);
						}
						else if (symbol == '>' || symbol == '=')
						{
							state = State.Comparison;
							next.Append(symbol);
						}
						else if (symbol == '+' || symbol == '-' || symbol == '/' || symbol == '*')
						{
							state = State.ArithmeticOperation;
							next.Append(symbol);
						}
						else
						{
							state = State.Error;
							AddLexemeFlag = false;
						}
						break;
					case State.Identifier:
                        if (char.IsWhiteSpace(symbol)) state = State.Start;
						else if (char.IsDigit(symbol) || char.IsLetter(symbol))
						{
							state = State.Identifier;
							AddLexemeFlag = false;
							current.Append(symbol);
						}
						else if (symbol == '<')
						{
							state = State.ReverseComparison;
							next.Append(symbol);
						}
						else if (symbol == '>' || symbol == '=')
						{
							state = State.Comparison;
							next.Append(symbol);
						}
						else if (symbol == '+' || symbol == '-' || symbol == '/' || symbol == '*')
						{
							state = State.ArithmeticOperation;
							next.Append(symbol);
						}
						else if (symbol == ':')
						{
							state = State.Assignment;
							next.Append(symbol);
						}
						else
						{
							state = State.Error;
							AddLexemeFlag = false;
						}
						break;
					case State.ArithmeticOperation:
						if (char.IsWhiteSpace(symbol)) state = State.Start;
						else if (char.IsLetter(symbol))
						{
							state = State.Identifier;
							next.Append(symbol);
						}
						else if (char.IsDigit(symbol))
						{
							state = State.Constant;
							next.Append(symbol);
						}
						else
						{
							state = State.Error;
							AddLexemeFlag = false;
						}
						break;
				}
				if (AddLexemeFlag && prevState != State.Start)
				{
					AddLexeme(prevState, current.ToString());
					current = new StringBuilder(next.ToString());
					next.Clear();
				}
                // pos in text by symbols
                else if (state == State.Error) Console.WriteLine("LexAnalysis: pos[{0}]", textIndex);
                textIndex++;
			}

			return state == State.Final;

		}

		private void AddLexeme(State state, string value)
		{
			LexemeType lexType = LexemeType.IdOrConst;
			LexemeClass lexClass = LexemeClass.IdOrConst;
			if (state == State.ArithmeticOperation)
			{
				lexType = LexemeType.ArithmeticOperation;
				lexClass = LexemeClass.SpecialSymbols;
			}
			else if (state == State.Assignment)
			{
				lexType = LexemeType.Assignment;
				lexClass = LexemeClass.SpecialSymbols;
			}
			else if (state == State.Constant)
			{
				lexType = LexemeType.IdOrConst;
				lexClass = LexemeClass.Constant;
			}
			else if (state == State.ReverseComparison)
			{
				lexType = LexemeType.Relation;
				lexClass = LexemeClass.SpecialSymbols;
			}
			else if (state == State.Comparison)
			{
				lexType = LexemeType.Relation;
				lexClass = LexemeClass.SpecialSymbols;
			}
			else if (state == State.Identifier)
			{
				bool isKeyword = true;
				if (value.ToLower() == "for") lexType = LexemeType.For;
				else if (value.ToLower() == "to") lexType = LexemeType.To;
				else if (value.ToLower() == "next") lexType = LexemeType.Next;
				else
				{
					lexType = LexemeType.IdOrConst;
					isKeyword = false;
				}
				if (isKeyword) lexClass = LexemeClass.Keyword;
				else lexClass = LexemeClass.Identifier;
			}
			var lexeme = new Lexeme
			{
				Class = lexClass,
				Type = lexType,
				Value = value,
			};
			Lexemes.Add(lexeme);
		}
		
		public bool SyntaxAnalysis()
		{
			return ForStatement();
		}

		private bool ForStatement()
		{
			indexLexems = 0;
			if (Lexemes[indexLexems].Type != LexemeType.For)
			{
				Console.WriteLine($"ForStatement: waiting Select, pos = {indexLexems}");
				return false;
			}
			++indexLexems;

			if (!Operator()) return false;

            // сохраняем начальное значение счётчика цикла из конструкции
            // используется для подобных случаев: for i = b + 1 to 5 a = a * 2 next
			// мы узнаем значение переменной b только при запуске интерперетатора,
			// поэтому пока сохраняем b + 1 в counterValue,
			// а затем в интерпретаторе подставим значение переменной b
			// и посчитаем счётчик
            for (int index = 1; index < indexLexems; ++index)
            {
                counterValue.Add(Lexemes[index].Value);
            }

            if (Lexemes[indexLexems].Type != LexemeType.To)
            {
                Console.WriteLine($"ForStatement: waiting Select, pos = {indexLexems}");
                return false;
            }
            ++indexLexems;

            string counter = Lexemes[1].Value;
            posForLoopCondition = poliz.Count;
            poliz.Add(counter);
            if (!AddSubExpr()) return false;
			poliz.Add("CMPLE");

			poliz.Add("END");
			poliz.Add("JZ");
			if (!Operator()) return false;

            // increment counter
            poliz.Add(counter);
            poliz.Add(counter);
            poliz.Add("1");
            poliz.Add("ADD");
            poliz.Add("SET");

			poliz.Add(Convert.ToString(posForLoopCondition));
			poliz.Add("JMP");

            if (Lexemes[indexLexems].Type != LexemeType.Next)
			{
                Console.WriteLine($"ForStatement: waiting End, pos = {indexLexems}");
                return false;
            }
			++indexLexems;

			if (indexLexems < Lexemes.Count)
			{
                Console.WriteLine($"ForStatement: unnecessary symbols, pos = {indexLexems}");
                return false;
            }

			// set address of END
			preparePOLIZ();
			return true;
        }

        private bool Operand()
        {
            if (Lexemes[indexLexems].Class != LexemeClass.Identifier && Lexemes[indexLexems].Class != LexemeClass.Constant)
            {
                Console.WriteLine($"Operand: waiting identifier or const, pos = {indexLexems}");
                return false;
            }

            // initializate variable for interpretator
            if (Lexemes[indexLexems].Class != LexemeClass.Constant && !varValues.ContainsKey(Lexemes[indexLexems].Value))
			{
                varValues[Lexemes[indexLexems].Value] = 0;
            }

            poliz.Add(Lexemes[indexLexems].Value);
            ++indexLexems;

			return true;
        }

		private bool MulDivExpr()
		{
            if (!Operand()) return false;

            string cmd = "";
            while (indexLexems < Lexemes.Count && (Lexemes[indexLexems].Value == "*" || Lexemes[indexLexems].Value == "/"))
            {
                cmd = (Lexemes[indexLexems].Value == "*") ? "MUL" : "DIV";

                ++indexLexems;
                if (!Operand()) return false;

                poliz.Add(cmd);
            }

            return true;
        }

        private bool AddSubExpr()
		{
			if (!MulDivExpr()) return false;

            string cmd = "";
            while (indexLexems < Lexemes.Count && (Lexemes[indexLexems].Value == "+" || Lexemes[indexLexems].Value == "-"))
			{
				cmd = (Lexemes[indexLexems].Value == "+") ? "ADD" : "SUB";

				++indexLexems;
                if (!MulDivExpr()) return false;

                poliz.Add(cmd);
            }

			return true;
		}

		private bool Operator()
		{
            if (Lexemes[indexLexems].Class != LexemeClass.Identifier)
            {
                Console.WriteLine($"Operator: waiting identifier, pos = {indexLexems}");
                return false;
            }

            poliz.Add(Lexemes[indexLexems].Value);
            ++indexLexems;

            if (Lexemes[indexLexems].Type != LexemeType.Assignment)
            {
                Console.WriteLine($"Operator: waiting assigment, pos = {indexLexems}");
                return false;
            }

            ++indexLexems;

			if (!AddSubExpr()) return false;
            poliz.Add("SET");

			return true;
        }

		private void preparePOLIZ()
		{
			for (int i = 0; i < poliz.Count; ++i)
			{
				if (poliz[i] == "END") poliz[i] = Convert.ToString(poliz.Count);
			}
		}

		private bool IsNumber(string str)
		{
			foreach (char c in str)
			{
				if (!char.IsDigit(c) && c != '-') return false;
			}

			return true;
		}

		private int PopVal()
		{
			if (stackInterpret.Count > 0)
			{
				if (IsNumber(stackInterpret.Peek()))
				{
					string val = stackInterpret.Peek();
					stackInterpret.Pop();
                    return Convert.ToInt32(val);
				}
				else
				{
                    int val = Convert.ToInt32(varValues[stackInterpret.Peek()]);
                    stackInterpret.Pop();
                    return val;
                }
			}

			return 0;
		}

		private void PushVal(int val)
		{
			stackInterpret.Push(Convert.ToString(val));
		}

		private void PushElem(string elem)
		{
			if (IsNumber(elem))
			{
				PushVal(Convert.ToInt32(elem));
			}
			else 
			{
				stackInterpret.Push(elem);
				if (!varValues.ContainsKey(elem)) varValues[elem] = 0;
			}
		}

		private void SetVarAndPop(int val)
		{
            varValues[stackInterpret.Peek()] = val;
            stackInterpret.Pop();
        }

		private string StackString()
		{
			string[] stackCopy = new string[stackInterpret.Count]; 
			stackInterpret.CopyTo(stackCopy, 0);
			string res = "";

			for (int i = 0; i < stackCopy.Length; ++i)
			{
				res += " ";
				res += stackCopy[i];
            }

			return res;
		}

		private string VarValuesString()
		{
			string res = "";
			foreach (var elem in varValues)
			{
				res += elem.Key;
				res += " = ";
				res += Convert.ToString(elem.Value);
				res += " ";
			}

			return res;
		}

		public void Interpret()
		{
			// set counter from ForStatement
			String counter = Lexemes[1].Value;
			varValues[counter] = 0;

			foreach (string value in counterValue)
			{
				if (IsNumber(value))
				{
					varValues[counter] += Convert.ToInt32(value);
				}
				else if (varValues.ContainsKey(value))
				{
					varValues[counter] += varValues[value];
                }
			}

			int val = 0, pos = 0, step = 0;

			var table = new ConsoleTable("step", "stack", "varValues");

			while (pos < poliz.Count)
			{
                if (poliz[pos] == "JMP") { pos = PopVal(); }
                else if (poliz[pos] == "JZ") { val = PopVal(); pos = Convert.ToBoolean(PopVal()) ? pos + 1: val; }
                else if (poliz[pos] == "SET") { SetVarAndPop(PopVal()); ++pos; }
                else if (poliz[pos] == "ADD") { PushVal(PopVal() + PopVal()); ++pos; }
                else if (poliz[pos] == "SUB") { PushVal(-PopVal() + PopVal()); ++pos; }
                else if (poliz[pos] == "MUL") { PushVal(PopVal() * PopVal()); ++pos; }
                else if (poliz[pos] == "DIV") { PushVal(PopVal() / PopVal()); ++pos; }
                else if (poliz[pos] == "CMPLE") { PushVal(Convert.ToInt32(PopVal() >= PopVal())); ++pos; }
                else { PushElem(poliz[pos++]); }

				table.AddRow(step, StackString(), VarValuesString());
                ++step;
            }

            table.Write();
        }
	}
}
