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
    tempos = new float[] { 12.314f, 11.914f, 14.614f, 11.614f } 
    },
    new TimeCinematico { 
        nomeEquipe = "Porco", 
        corTronco = new Color(1f, 0.9f, 0f), 
        raia = 3, 
        tempos = new float[] { 12.246f, 12.446f, 13.546f, 12.346f } 
    },
    new TimeCinematico { 
        nomeEquipe = "Raposa", 
        corTronco = new Color(0f, 0f, 0.6f), 
        raia = 2, 
        tempos = new float[] { 12.993f, 12.793f, 12.893f, 12.893f } 
    },
    new TimeCinematico { 
        nomeEquipe = "Pato", 
        corTronco = new Color(0f, 0f, 0.6f), 
        raia = 5, 
        tempos = new float[] { 13.220f, 13.220f, 13.220f, 13.220f } 
    },
    new TimeCinematico { 
        nomeEquipe = "Medicina Fag", 
        corTronco = new Color(0f, 0f, 0.6f), 
        raia = 6, 
        tempos = new float[] { 13.547f, 13.547f, 13.547f, 13.547f } 
    },
    new TimeCinematico { 
        nomeEquipe = "Venenosa", 
        corTronco = new Color(0f, 0f, 0.6f), 
        raia = 1, 
        tempos = new float[] { 13.595f, 13.595f, 13.595f, 13.595f } 
    },
    new TimeCinematico { 
        nomeEquipe = "Ardilosa", 
        corTronco = new Color(0f, 0f, 0.6f), 
        raia = 8, 
        tempos = new float[] { 13.759f, 13.759f, 13.759f, 13.759f } 
    },
    new TimeCinematico { 
        nomeEquipe = "Rino", 
        corTronco = new Color(0f, 0f, 0.6f), 
        raia = 7, 
        tempos = new float[] { 14.085f, 14.085f, 14.085f, 14.085f } 
    },

    };
}