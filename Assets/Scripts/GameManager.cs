using UnityEngine;
using System.Collections.Generic;

public static class GameSettings
{
    public enum Difficulty { Facil, Medio, Dificil }
    public static Difficulty dificuldadeSelecionada = Difficulty.Medio;
    public static SkeletonRacer.TipoDeProva provaSelecionada = SkeletonRacer.TipoDeProva.Sprint_100m;
    public static int raiaDoJogador = 3;
    public static bool modoAutomatico = false;
    public static bool usarAutoClick = false;
    public static float clicksPorSegundo = 9f;
    public static bool isCinematicMode = false;

    public struct TimeCinematico {
        public string nomeEquipe;
        public Color corTronco;
        public int raia;
        public float[] tempos; 
    }

    public static List<TimeCinematico> timesReplay = new List<TimeCinematico>()
    {
        new TimeCinematico { 
            nomeEquipe = "Gorila", 
            corTronco = new Color(0f, 0.5f, 1f), 
            raia = 4, 
            tempos = new float[] { 10.5f, 9.8f, 10.2f, 9.5f } 
        },
        new TimeCinematico { 
            nomeEquipe = "Porco", 
            corTronco = new Color(1f, 0.9f, 0f), 
            raia = 3, 
            tempos = new float[] { 10.2f, 9.9f, 9.8f, 9.0f } 
        },
        new TimeCinematico { 
            nomeEquipe = "EeeeeeeeeeUA", 
            corTronco = new Color(0f, 0f, 0.6f), 
            raia = 5, 
            tempos = new float[] { 10.3f, 10.0f, 9.9f, 9.2f } 
        },
        new TimeCinematico { 
            nomeEquipe = "EeeeeeUA", 
            corTronco = new Color(0f, 0f, 0.6f), 
            raia = 6, 
            tempos = new float[] { 10.3f, 10.0f, 9.9f, 9.2f } 
        },
        new TimeCinematico { 
            nomeEquipe = "EeeeeUA", 
            corTronco = new Color(0f, 0f, 0.6f), 
            raia = 2, 
            tempos = new float[] { 10.3f, 10.0f, 9.9f, 9.2f } 
        },
        new TimeCinematico { 
            nomeEquipe = "EeeeUA", 
            corTronco = new Color(0f, 0f, 0.6f), 
            raia = 7, 
            tempos = new float[] { 10.3f, 10.0f, 9.9f, 9.2f } 
        },
        new TimeCinematico { 
            nomeEquipe = "EeeUA", 
            corTronco = new Color(0f, 0f, 0.6f), 
            raia = 8, 
            tempos = new float[] { 10.3f, 10.0f, 9.9f, 9.2f } 
        },
        new TimeCinematico { 
            nomeEquipe = "EeUA", 
            corTronco = new Color(0f, 0f, 0.6f), 
            raia = 1, 
            tempos = new float[] { 10.3f, 10.0f, 9.9f, 9.2f } 
        },
    };
}