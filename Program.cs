//Bruno Branco dos Santos
using System;
using System.Text;
using System.Collections.Generic;
namespace BatalhaNaval
{
    enum Navio { Nenhum, PortaAvioes, Rebocador, Cruzador }

    class Tiro
    {
        public string Coordenada;
        public Navio Acertou;
        public char Erro;
    }

    class Partida
    {
        public const int Tamanho = 10;
        public const int MaxTentativas = 15;

        static readonly Random Rnd = new Random();

        public Navio[,] Navios = new Navio[Tamanho, Tamanho]; 
        public bool[,] Abatido = new bool[Tamanho, Tamanho]; 
        public bool[,] Jogado = new bool[Tamanho, Tamanho];
        public char[,] Erro = new char[Tamanho, Tamanho]; 
        public List<Tiro> Tiros = new List<Tiro>();
        public int Pontos;
        public int Tentativas;

        public Partida()
        {
            Posicionar(Navio.PortaAvioes, 10);
            Posicionar(Navio.Cruzador, 1);
            Posicionar(Navio.Rebocador, 2);
        }

        void Posicionar(Navio tipo, int quantidade)
        {
            int colocados = 0;
            while (colocados < quantidade)
            {
                int l = Rnd.Next(Tamanho);
                int c = Rnd.Next(Tamanho);
                if (Navios[l, c] == Navio.Nenhum)
                {
                    Navios[l, c] = tipo;
                    colocados++;
                }
            }
        }

        public static int Valor(Navio n)
        {
            switch (n)
            {
                case Navio.PortaAvioes: return 5;
                case Navio.Rebocador: return 10;
                case Navio.Cruzador: return 15;
                default: return 0;
            }
        }

        public static string Nome(Navio n)
        {
            switch (n)
            {
                case Navio.PortaAvioes: return "porta-aviões";
                case Navio.Rebocador: return "rebocador";
                case Navio.Cruzador: return "cruzador";
                default: return "";
            }
        }

        public static char Letra(Navio n)
        {
            switch (n)
            {
                case Navio.PortaAvioes: return 'P';
                case Navio.Rebocador: return 'R';
                case Navio.Cruzador: return 'C';
                default: return '.';
            }
        }

        public bool TodosAbatidos()
        {
            for (int l = 0; l < Tamanho; l++)
                for (int c = 0; c < Tamanho; c++)
                    if (Navios[l, c] != Navio.Nenhum && !Abatido[l, c])
                        return false;
            return true;
        }

        public Navio Atirar(int l, int c, string coordenada)
        {
            Jogado[l, c] = true;
            Tentativas++;

            Tiro tiro = new Tiro();
            tiro.Coordenada = coordenada;

            if (Navios[l, c] != Navio.Nenhum)
            {
                Abatido[l, c] = true;
                Pontos += Valor(Navios[l, c]);
                tiro.Acertou = Navios[l, c];
            }
            else
            {
                Erro[l, c] = CalcularErro(l, c);
                tiro.Acertou = Navio.Nenhum;
                tiro.Erro = Erro[l, c];
            }

            Tiros.Add(tiro);
            return tiro.Acertou;
        }

        char CalcularErro(int l, int c)
        {
            int[] dl = { 0, 0, -1, 1 };
            int[] dc = { -1, 1, 0, 0 };

            for (int d = 1; d <= 3; d++)
            {
                for (int k = 0; k < 4; k++)
                {
                    int nl = l + dl[k] * d;
                    int nc = c + dc[k] * d;
                    bool dentro = nl >= 0 && nl < Tamanho && nc >= 0 && nc < Tamanho;
                    if (dentro && Navios[nl, nc] != Navio.Nenhum && !Abatido[nl, nc])
                        return (char)('0' + d);
                }
            }
            return 'M';
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            bool jogarNovamente;
            do
            {
                Jogar();
                jogarNovamente = PerguntarNovaPartida();
            } while (jogarNovamente);

            Console.WriteLine("Saindo");
        }

        static void Jogar()
        {
            Partida p = new Partida();
            string mensagem = "";
            ConsoleColor corMensagem = ConsoleColor.Gray;

            while (p.Tentativas < Partida.MaxTentativas && !p.TodosAbatidos())
            {
                Limpar();
                Console.WriteLine("\n=== BATALHA NAVAL ===");
                Console.WriteLine("Tentativa {0} de {1}   |   Pontos: {2}",
                    p.Tentativas + 1, Partida.MaxTentativas, p.Pontos);
                Console.WriteLine();
                Desenhar(p, false);
                Legenda(false);

                if (mensagem != "")
                {
                    Escrever(mensagem + "\n", corMensagem);
                    Console.WriteLine();
                }

                int l, c;
                string coord = LerCoordenada(p, out l, out c);
                if (coord == null) return;

                Navio acertou = p.Atirar(l, c, coord);
                if (acertou != Navio.Nenhum)
                {
                    mensagem = string.Format("{0}: ACERTOU um {1}! +{2} pontos.",
                        coord, Partida.Nome(acertou), Partida.Valor(acertou));
                    corMensagem = ConsoleColor.Red;
                }
                else
                {
                    char e = p.Erro[l, c];
                    if (e == 'M')
                        mensagem = string.Format("{0}: Água! Errou por muito (M).", coord);
                    else
                        mensagem = string.Format("{0}: Água! Tem navio a {1} casa de distância.", coord, e);
                    corMensagem = ConsoleColor.Green;
                }
            }

            TelaFinal(p, mensagem, corMensagem);
        }

        static string LerCoordenada(Partida p, out int linha, out int coluna)
        {
            linha = 0;
            coluna = 0;

            while (true)
            {
                Console.Write("Informe a coordenada que deseja jogar: ");
                string entrada = Console.ReadLine();
                if (entrada == null) return null;

                string t = entrada.Replace(" ", "").ToUpper();
                int num;

                if (t.Length < 2 || t[0] < 'A' || t[0] >= 'A' + Partida.Tamanho
                    || !int.TryParse(t.Substring(1), out num)
                    || num < 1 || num > Partida.Tamanho)
                {
                    Console.WriteLine("Coordenada inválida. Use uma letra de A a J e um número de 1 a 10.");
                    continue;
                }

                coluna = t[0] - 'A';
                linha = num - 1;

                if (p.Jogado[linha, coluna])
                {
                    Console.WriteLine("Você já jogou uma bomba em {0}. Escolha outra coordenada.", t);
                    continue;
                }

                return t;
            }
        }

        static void Desenhar(Partida p, bool revelar)
        {
            Console.Write("     ");
            for (int c = 0; c < Partida.Tamanho; c++)
                Console.Write((char)('A' + c) + " ");
            Console.WriteLine();

            for (int l = 0; l < Partida.Tamanho; l++)
            {
                Console.Write((l + 1).ToString().PadLeft(3) + "  ");
                for (int c = 0; c < Partida.Tamanho; c++)
                {
                    if (p.Abatido[l, c])
                        Escrever(Partida.Letra(p.Navios[l, c]) + " ", ConsoleColor.Red);
                    else if (p.Erro[l, c] != '\0')
                        Escrever(p.Erro[l, c] + " ", ConsoleColor.Green);
                    else if (revelar && p.Navios[l, c] != Navio.Nenhum)
                        Escrever(Partida.Letra(p.Navios[l, c]) + " ", ConsoleColor.Cyan);
                    else
                        Escrever(". ", ConsoleColor.DarkGray);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        static void Legenda(bool final)
        {
            Console.Write("Navios: P = porta-aviões, R = rebocador, C = cruzador \n");
            Escrever("Vermelho", ConsoleColor.Red);
            Console.WriteLine(" = abatido");
            Escrever("Verde", ConsoleColor.Green);
            Console.Write(" = água \n(1, 2 ou 3 = navio a essa distância; M = errou por muito)");
            if (final)
            {
                Console.Write("   ");
                Escrever("Azul", ConsoleColor.Cyan);
                Console.Write(" = navio que restou");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        static void TelaFinal(Partida p, string mensagem, ConsoleColor corMensagem)
        {
            Limpar();
            Console.WriteLine("=== FIM DE JOGO ===");
            Console.WriteLine();
            Desenhar(p, true);
            Legenda(true);

            if (mensagem != "")
            {
                Escrever(mensagem + "\n", corMensagem);
                Console.WriteLine();
            }

            Console.WriteLine("Bombas lançadas:");
            for (int i = 0; i < p.Tiros.Count; i++)
            {
                Tiro t = p.Tiros[i];
                string num = (i + 1).ToString().PadLeft(2);
                if (t.Acertou != Navio.Nenhum)
                    Escrever(string.Format("  {0}. {1,-4} acertou um {2}\n",
                        num, t.Coordenada, Partida.Nome(t.Acertou)), ConsoleColor.Red);
                else
                    Escrever(string.Format("  {0}. {1,-4} água ({2})\n",
                        num, t.Coordenada, t.Erro), ConsoleColor.Green);
            }

            Console.WriteLine();
            Console.WriteLine("Pontuação final: {0} pontos", p.Pontos);
            Console.WriteLine();
        }

        static bool PerguntarNovaPartida()
        {
            while (true)
            {
                Console.Write("Digite N para uma nova partida ou S para sair: ");
                string r = Console.ReadLine();
                if (r == null) return false;
                r = r.Trim().ToUpper();
                if (r == "N") return true;
                if (r == "S") return false;
                Console.WriteLine("Opção inválida.");
            }
        }

        static void Escrever(string texto, ConsoleColor cor)
        {
            ConsoleColor anterior = Console.ForegroundColor;
            Console.ForegroundColor = cor;
            Console.Write(texto);
            Console.ForegroundColor = anterior;
        }

        static void Limpar()
        {
            try { Console.Clear(); }
            catch (System.IO.IOException) { }
        }
    }
}