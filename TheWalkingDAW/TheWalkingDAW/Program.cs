using System.Text;
using static System.Console;

OutputEncoding = Encoding.UTF8;

// Variables (declaradas antes de llamar a AsignarVariables para que no dé error)
int tamaño = 0;
int infectados = 0; // Los asignamos un 1
int sanos = 0;      // Los asignamos un 2
int tiempo = 0;
int ciclosTotales = 0;
int zombiesEliminados = 0;

const double ProbContagio = 0.7;
const double ProbMatar = 0.2;
const double ProbMuerte = 0.1;

AsignarVariables();

// Matriz
int[,] matrizOriginal = new int[tamaño, tamaño];

RellenarMatriz(matrizOriginal);
WriteLine("\nEstado inicial:");
MostrarArray(matrizOriginal);

// Aquí empieza el juego
Simulación(matrizOriginal);
return;

void AsignarVariables()
{
    // Asignamos tamaño
    do
    {
        Write("Tamaño de la clase: ");
        tamaño = int.TryParse(ReadLine()?.Trim(), out int t) ? t : 0;
        if (tamaño <= 0)
        {
            WriteLine("Haz la clase más grande");
        }
    } while (tamaño <= 0);

    // Asignamos zombies
    do
    {
        Write("Número de infectados: ");
        infectados = int.TryParse(ReadLine()?.Trim(), out int i) ? i : 0;
        if (infectados <= 0)
            WriteLine("Añade al menos 1 infectado");
        else if (infectados >= tamaño)
            WriteLine("No caben tantos zombies, añade un número más pequeño");
    } while (infectados <= 0 || infectados >= tamaño);

    // Asignamos sanos
    do
    {
        Write("Número de sanos: ");
        sanos = int.TryParse(ReadLine()?.Trim(), out int s) ? s : 0;
        if (sanos <= 0)
            WriteLine("Añade al menos 1 sano");
        else if (sanos >= tamaño)
            WriteLine("No caben tantos sanos, añade un número más pequeño");
    } while (sanos <= 0 || sanos >= tamaño);

    // Asignamos el tiempo total
    do
    {
        Write("Turnos totales: ");
        tiempo = int.TryParse(ReadLine()?.Trim(), out int t) ? t : 0;
        if (tiempo <= 0)
            WriteLine("Añade al menos 1 turno");
        else if (tiempo >= 50)
            WriteLine("Demasiados turnos (máximo 50)");
    } while (tiempo >= 50 || tiempo <= 0);
}

void RellenarMatriz(int[,] matriz)
{
    for (int contadorInfectados = infectados; contadorInfectados > 0; contadorInfectados--)
    {
        int x = Random.Shared.Next(0, tamaño);
        int y = Random.Shared.Next(0, tamaño);
        if (matriz[x, y] == 0) // Corregido: solo colocar si la casilla está libre
        {
            matriz[x, y] = 1;
        }
        else
        {
            contadorInfectados++;
        }
    }

    for (int contadorSanos = sanos; contadorSanos > 0; contadorSanos--)
    {
        int x = Random.Shared.Next(0, tamaño);
        int y = Random.Shared.Next(0, tamaño);
        if (matriz[x, y] == 0) // Corregido: solo colocar si la casilla está libre
        {
            matriz[x, y] = 2;
        }
        else
        {
            contadorSanos++;
        }
    }
}

void MostrarArray(int[,] matriz)
{
    for (int i = 0; i < matriz.GetLength(0); i++)
    {
        for (int j = 0; j < matriz.GetLength(1); j++)
        {
            if (matriz[i, j] == 1) // Zombie
            {
                Write("[🧟]");
            }
            else if (matriz[i, j] == 2) // Sano
            {
                Write("[🧑]");
            }
            else
            {
                Write("[  ]");
            }
        }
        WriteLine();
    }
}

void Simulación(int[,] matriz)
{
    // Continúa mientras quede tiempo, quede al menos 1 zombie y quede al menos 1 sano
    while (tiempo > 0 && Contar(matriz, 1) > 0 && Contar(matriz, 2) > 0)
    {
        Thread.Sleep(2000);
        tiempo--;
        ciclosTotales++;

        Ciclos();
        Clear();

        WriteLine($"\n--- Ciclo {ciclosTotales} ---");
        MostrarArray(matriz);
    }
    Thread.Sleep(2000);
    MostrarResultadoFinal(matriz);
}

void Ciclos()
{
    ComportamientoZombie(matrizOriginal);
    ComportamientoPersona(matrizOriginal);
}

void ComportamientoZombie(int[,] matriz)
{
    // Evita que un zombie que se mueva hacia abajo o a la derecha vuelva a actuar en el mismo ciclo
    bool[,] yaActuo = new bool[tamaño, tamaño];

    for (int i = 0; i < matriz.GetLength(0); i++)
    {
        for (int j = 0; j < matriz.GetLength(1); j++)
        {
            if (matriz[i, j] == 1 && !yaActuo[i, j]) // Si estamos en "Zombie"
            {
                if (Random.Shared.NextDouble() < ProbMuerte) // Si sale, muere el zombie
                {
                    matriz[i, j] = 0; // Lo dejamos libre
                    zombiesEliminados++;
                }
                else
                {
                    // 1. MOVERSE (hasta 25 intentos para encontrar casilla libre adyacente)
                    int nuevoX = i;
                    int nuevoY = j;

                    for (int intentos = 0; intentos < 25; intentos++)
                    {
                        int cx = i + Random.Shared.Next(-1, 2);
                        int cy = j + Random.Shared.Next(-1, 2);

                        if (EstáDentroDeLaMatriz(cx, cy) && matriz[cx, cy] == 0)
                        {
                            nuevoX = cx;
                            nuevoY = cy;
                            break;
                        }
                    }

                    matriz[i, j] = 0;           // Dejamos la casilla anterior libre
                    matriz[nuevoX, nuevoY] = 1; // Movemos al zombie
                    yaActuo[nuevoX, nuevoY] = true;

                    // 2. COMPORTAMIENTO ATACAR
                    for (int df = -1; df <= 1; df++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            if (df == 0 && dc == 0) continue;

                            int nuevaFila = nuevoX + df;
                            int nuevaColumna = nuevoY + dc;

                            if (EstáDentroDeLaMatriz(nuevaFila, nuevaColumna) && matriz[nuevaFila, nuevaColumna] == 2)
                            {
                                if (Random.Shared.NextDouble() < ProbContagio)
                                {
                                    matriz[nuevaFila, nuevaColumna] = 1; // Se convierte en zombie
                                    yaActuo[nuevaFila, nuevaColumna] = true; // Actuará en el siguiente ciclo
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

void ComportamientoPersona(int[,] matriz)
{
    bool[,] yaActuo = new bool[tamaño, tamaño];

    for (int i = 0; i < matriz.GetLength(0); i++)
    {
        for (int j = 0; j < matriz.GetLength(1); j++)
        {
            if (matriz[i, j] == 2 && !yaActuo[i, j]) // Si estamos en "Persona Sana"
            {
                // 1. MOVERSE a una casilla libre adyacente
                int nuevoX = i;
                int nuevoY = j;

                for (int intentos = 0; intentos < 25; intentos++)
                {
                    int cx = i + Random.Shared.Next(-1, 2);
                    int cy = j + Random.Shared.Next(-1, 2);

                    if (EstáDentroDeLaMatriz(cx, cy) && matriz[cx, cy] == 0)
                    {
                        nuevoX = cx;
                        nuevoY = cy;
                        break;
                    }
                }

                matriz[i, j] = 0;
                matriz[nuevoX, nuevoY] = 2;
                yaActuo[nuevoX, nuevoY] = true;

                // 2. DEFENSA: Si tiene uno o más ZC adyacentes, probabilidad ProbMatar de matar a 1 al azar
                int zx = -1, zy = -1;
                int zombiesCerca = 0;

                for (int df = -1; df <= 1; df++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (df == 0 && dc == 0) continue;

                        int nuevaFila = nuevoX + df;
                        int nuevaColumna = nuevoY + dc;

                        if (EstáDentroDeLaMatriz(nuevaFila, nuevaColumna) && matriz[nuevaFila, nuevaColumna] == 1)
                        {
                            zombiesCerca++;
                            if (Random.Shared.Next(0, zombiesCerca) == 0)
                            {
                                zx = nuevaFila;
                                zy = nuevaColumna;
                            }
                        }
                    }
                }

                if (zombiesCerca > 0 && Random.Shared.NextDouble() < ProbMatar)
                {
                    matriz[zx, zy] = 0; // Mata al zombie y deja su zona libre
                    zombiesEliminados++;
                }
            }
        }
    }
}

bool EstáDentroDeLaMatriz(int x, int y)
{
    if (x < 0 || x >= matrizOriginal.GetLength(0) || y < 0 || y >= matrizOriginal.GetLength(1))
        return false;

    return true;
}

int Contar(int[,] matriz, int tipo)
{
    int contador = 0;
    for (int i = 0; i < matriz.GetLength(0); i++)
    {
        for (int j = 0; j < matriz.GetLength(1); j++)
        {
            if (matriz[i, j] == tipo) contador++;
        }
    }
    return contador;
}

void MostrarResultadoFinal(int[,] matriz)
{
    int zombiesRestantes = Contar(matriz, 1);
    int sanosRestantes = Contar(matriz, 2);

    WriteLine();
    WriteLine("==============================");
    WriteLine("      FIN DE LA SIMULACIÓN    ");
    WriteLine("==============================");
    MostrarArray(matriz);
    WriteLine();

    if (zombiesRestantes == 0)
        WriteLine(" VICTORIA HUMANA: No quedan Zombies.");
    else if (sanosRestantes == 0)
        WriteLine(" VICTORIA DE LOS ZOMBIES: No quedan Personas Sanas.");
    else
        WriteLine(" LÍMITE DE TIEMPO: Se agotaron los ciclos.");

    WriteLine($"\nEstadísticas:");
    WriteLine($"- Ciclos totales: {ciclosTotales}");
    WriteLine($"- ZC eliminados: {zombiesEliminados}");
    WriteLine($"- Sanos supervivientes: {sanosRestantes}");
}