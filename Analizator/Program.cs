using ConsoleTables;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace Analizators
{
	class Program
	{
		static void Main(string[] args)
		{
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            using var stream = new StreamReader("C:\\Users\\sorok\\Downloads\\Eremeev\\Analizator\\Analizator\\input.txt");
            var text = stream.ReadToEnd();

            var analyzer = new Analizator();
            if (!analyzer.LexicalAnalysis(string.Join(Environment.NewLine, text)))
            {
                Console.WriteLine("LexAnalysis: errors were occurred");
                return;
            }
            Console.WriteLine("LexAnalysis: succsess");
            var lexemes = analyzer.Lexemes;
            var tableLexems = new ConsoleTable("index", "class", "type", "value");
            for (int i = 0; i < lexemes.Count; i++)
            {
                tableLexems.AddRow(Convert.ToString(i), lexemes[i].Class, lexemes[i].Type, lexemes[i].Value);
            }
            tableLexems.Write();

            Console.WriteLine(analyzer.SyntaxAnalysis() ? "SyntaxAnalys: Success" : "SyntaxAnalys: Fault");

            Console.WriteLine("poliz:");

            foreach (string elem in analyzer.poliz)
            {
                Console.Write(elem + " ");
            }

            Console.WriteLine();

            var table = new ConsoleTable("indexLexems", "POLIZ");
            for (int i = 0; i < analyzer.poliz.Count; ++i)
            {
                table.AddRow(Convert.ToString(i), analyzer.poliz[i]);
            }
            table.Write();

            Console.WriteLine("interpretator:");
            foreach (string key in analyzer.varValues.Keys)
            {
                Console.Write(key + " = ");
                analyzer.varValues[key] = Convert.ToInt32(Console.ReadLine());
            }
            analyzer.Interpret();

        }
    }
}
