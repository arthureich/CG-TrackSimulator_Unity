using UnityEngine;
using UnityEngine.UI;

public class SkeletonRacer : MonoBehaviour
{
    [Header("Identidade")]
    public bool isBot = false; 
    public string nomeCorredor = "Player";

    // --- MÁQUINA DE ESTADOS ---
    public enum RaceState { Ready, Set, GunFired, Running, FalseStart, Finished, WaitingHandoff}
    [Header("🚦 Estado da Prova")]
    public RaceState raceState = RaceState.Ready; private float _timerState = 0f; private float _timeWhenGunFired = 0f;
    private float _delayToSet = 1.5f; private float _delayToGun = 2.0f; 
    [Header("Ambiente")]
    public TrackManager trackManager; public int raiaAlvo = 3; public float offsetLateral = -0.6f; private float _offsetLargada = 0f;
    // --- CONFIGURAÇÕES ---
    public enum TipoDeProva { Sprint_100m, Sprint_200m, SprintLongo_400m, MeioFundo_800m, MeioFundo_1500m, Revezamento_4x100m}
    [Header("Configuração da Prova")]
    public TipoDeProva provaSelecionada = TipoDeProva.Sprint_100m; public float distanciaTotalDaProva = 100f;
    [Header("Controles")]
    public bool modoAutomatico = false; [Range(0.1f, 1.5f)] public float ritmoAutomatico = 0.95f; public bool usarAutoClick = false; 
    [Range(1f, 15f)] public float clicksPorSegundo = 9f; private float _timerAutoClick = 0f; public float tempoReacaoAuto = 0.14f;
    [Header("Física")]
    public float velocidadeMaximaCap = 12.5f; public float forcaBaseClique = 1.0f; public float arrastoBase = 1.0f;          
    public float resistenciaAoLimite = 0.9f; public float potenciaCurvaForca = 1.0f; public float limiarArrancada = 5.0f; 
    public float forcaInicialMinima = 0.5f; public float arrastoDinamicoLinear = 0.0f; public float arrastoDinamicoQuadratico = 0.0f; 
    [Header("Stamina")]
    public bool usarFadiga = true; public float staminaConsumoBase = 0f; public float staminaConsumoLinear = 0f; public float staminaConsumoCubico = 0f;    
    public float staminaConsumoExcesso = 0f; [Range(0, 100)] public float stamina = 100f; public float targetPace = 0f; public float taxaRecuperacao = 5.0f; 
    public float limiarFadiga = 30f; public float penalidadeFadigaMin = 0.25f; public bool fadigaQuadratica = false; public bool fadigaDupla = false;
    [Header("Revezamento")]
    public SkeletonRacer colegaAnterior; public float distanciaGatilho; public int numeroPerna = 1; 
    // --- ESTADO PÚBLICO ---
    public float velocidadeAtual = 0f; public float distanciaPercorridaTotal = 0f; public float tempoCorrida = 0f; public float tempoReacaoReal = 0f;
    [Header("Cinematico")]
    public float tempoAlvo = 0f; public Color corForcada = Color.clear;
    // Telemetria interna
    private float t_10m, t_20m, t_30m, t_40m, t_100m, t_200m, t_400m, t_800m, t_1500m;
    private bool f10, f20, f30, f40, f100, f200, f400, f800, f1500;
    [Header("UI Referências")]
    public Slider uiVelocidade; public Slider uiFadiga; public Image uiCorFadiga;

    void Start() 
    { 
        if (GameSettings.isCinematicMode)
        {
            stamina = 100f;
            PosicionarNaPista();
            var builder = GetComponent<SkeletonBuilder>();
            if (builder) builder.Start();
            return; 
        }
        if (raceState == RaceState.WaitingHandoff)
        {
            PosicionarNaPista();
            return; 
        }
        if (!isBot)
        {
            provaSelecionada = GameSettings.provaSelecionada;
            raiaAlvo = GameSettings.raiaDoJogador;
            modoAutomatico = GameSettings.modoAutomatico;
            usarAutoClick = GameSettings.usarAutoClick;
            clicksPorSegundo = GameSettings.clicksPorSegundo; 
            nomeCorredor = "Player";
        }

        ConfigurarProva(); 
        stamina = 100f;
        raceState = RaceState.Ready;
        _timerState = 0f;

        if (provaSelecionada == TipoDeProva.MeioFundo_800m || provaSelecionada == TipoDeProva.MeioFundo_1500m) {
            _delayToSet = 0.5f; _delayToGun = 1.0f;
        }
        PosicionarNaPista();
    }
    
    void OnValidate() { ConfigurarProva(); }

    public void ConfigurarProva()
    {
        tempoCorrida = 0f; float distanciaDaProva = 100f; distanciaTotalDaProva = 100f;
        staminaConsumoBase = 0; staminaConsumoLinear = 0; staminaConsumoCubico = 0; staminaConsumoExcesso = 0;
        arrastoDinamicoLinear = 0; arrastoDinamicoQuadratico = 0;
        potenciaCurvaForca = 1.0f; resistenciaAoLimite = 0.909f;
        fadigaQuadratica = false; fadigaDupla = false; limiarArrancada = 4.0f; forcaInicialMinima = 0.5f;

        switch (provaSelecionada)
        {
            case TipoDeProva.Sprint_100m:
                velocidadeMaximaCap = 12.4f; forcaBaseClique = 0.88f; arrastoBase = 0.0f; arrastoDinamicoQuadratico = 0.0122f; distanciaTotalDaProva = 100f;
                resistenciaAoLimite = 0.76f; limiarArrancada = 3f; forcaInicialMinima = 1.34f; targetPace = 12.38f; distanciaDaProva = 100f; clicksPorSegundo = 8.5f; break;
            case TipoDeProva.Sprint_200m:
                velocidadeMaximaCap = 12.4f; forcaBaseClique = 0.8f; arrastoBase = 0.0f; arrastoDinamicoLinear = 0.058f; staminaConsumoExcesso = 0.75f;
                staminaConsumoLinear = 0.575f; limiarFadiga = 30f; penalidadeFadigaMin = 0.0001f; fadigaQuadratica = true; clicksPorSegundo = 7.5f;
                limiarArrancada = 3f; forcaInicialMinima = 1.34f; targetPace = 12.38f; distanciaDaProva = 200f; distanciaTotalDaProva = 200f; break;
            case TipoDeProva.SprintLongo_400m:
                velocidadeMaximaCap = 12f; forcaBaseClique = 1f; arrastoBase = 0.75f; arrastoDinamicoLinear = 0.075f; clicksPorSegundo = 7.5f;
                staminaConsumoBase = 0.2f; staminaConsumoLinear = 0.1f; staminaConsumoCubico = 0.001f; distanciaTotalDaProva = 400f;
                limiarFadiga = 25f; penalidadeFadigaMin = 0.0001f; fadigaDupla = true; distanciaDaProva = 400f; staminaConsumoExcesso = 0.75f;
                limiarArrancada = 3f; forcaInicialMinima = 1.34f; targetPace = 9.7f; taxaRecuperacao = 3f; break;
            case TipoDeProva.MeioFundo_800m:
                velocidadeMaximaCap = 11.5f; forcaBaseClique = 0.94f; arrastoBase = 0.1f; arrastoDinamicoLinear = 0.045f; clicksPorSegundo = 5f;
                staminaConsumoBase = 0.3f; staminaConsumoLinear = 0.09f; staminaConsumoExcesso = 1.0f; distanciaDaProva = 800f;
                targetPace = 8.1f; limiarArrancada = 3f; forcaInicialMinima = 1.34f; taxaRecuperacao = 2f; distanciaTotalDaProva = 800f;break;
            case TipoDeProva.MeioFundo_1500m:
                velocidadeMaximaCap = 11.5f; forcaBaseClique = 0.94f; arrastoBase = 0.1f; arrastoDinamicoLinear = 0.06f; clicksPorSegundo = 5f;
                staminaConsumoBase = 0.02f; staminaConsumoLinear = 0.065f; staminaConsumoExcesso = 0.3f; distanciaTotalDaProva = 1500f;
                targetPace = 7.4f; taxaRecuperacao = 0.8f; limiarArrancada = 3f; forcaInicialMinima = 1.34f; distanciaDaProva = 1500f; break;
            case TipoDeProva.Revezamento_4x100m:
                velocidadeMaximaCap = 12.5f; forcaBaseClique = 0.95f; arrastoBase = 0.0f; arrastoDinamicoQuadratico = 0.012f; 
                distanciaTotalDaProva = 400f; staminaConsumoLinear = 0f; staminaConsumoCubico = 0f; resistenciaAoLimite = 0.76f; 
                limiarArrancada = 3f; forcaInicialMinima = 1.34f; targetPace = 12.5f; distanciaDaProva = 100f; 
                clicksPorSegundo = 9f; break;
        }

        if (trackManager != null) {
            _offsetLargada = trackManager.GetPontoDeLargada(distanciaTotalDaProva, raiaAlvo);
            if (provaSelecionada != TipoDeProva.Revezamento_4x100m || numeroPerna == 1)
            {
                distanciaPercorridaTotal = 0f; 
            }
        }
    }

    public void OuvirComandoSet()
    {
        if (raceState == RaceState.Ready)
        {
            raceState = RaceState.Set;
        }
    }

    public void OuvirTiroDeLargada()
    {
        if (raceState == RaceState.Set || raceState == RaceState.Ready)
        {
            raceState = RaceState.GunFired;
            _timeWhenGunFired = Time.time; 
        }
    }

    void Update()
    {
        if (raceState == RaceState.WaitingHandoff)
        {
            if (colegaAnterior != null && colegaAnterior.distanciaPercorridaTotal >= distanciaGatilho)
            {
                StartRunning(0.1f); 
                _timeWhenGunFired = colegaAnterior._timeWhenGunFired;
                if (!isBot)
                {
                    var manager = FindObjectOfType<RaceManager>();
                    if (manager) manager.TrocarFocoCamera(this);
                }
            }
            return;
        }

        if (raceState == RaceState.GunFired)
        {
            ProcessarReacao();
        }
        if (raceState == RaceState.Running) {
            UpdateFisica();
            tempoCorrida = Time.time - _timeWhenGunFired;
            if (provaSelecionada == TipoDeProva.Revezamento_4x100m && numeroPerna < 4)
            {
                float limitePerna = numeroPerna * 100f - 3f; 
                if (GameSettings.isCinematicMode) {
                    limitePerna = numeroPerna * 100f;
                }
                if (distanciaPercorridaTotal > limitePerna)
                {
                    raceState = RaceState.Finished; 
                }
            }
            if (distanciaPercorridaTotal >= distanciaTotalDaProva)
            {
                FinalizarProva();
            }
        } 
        else if (raceState == RaceState.Finished)
        {
            velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, 0f, Time.deltaTime * 6f);
        }
        else {
            velocidadeAtual = 0f; 
        }
        UpdateMovimento();
        if (!isBot) UpdateTelemetria();
        if (uiVelocidade) uiVelocidade.value = velocidadeAtual/velocidadeMaximaCap; 
        if (uiFadiga) { uiFadiga.value = stamina/100f; if (uiCorFadiga) uiCorFadiga.color = Color.Lerp(Color.red, Color.green, stamina/100f); }
    }

    void FinalizarProva()
    {
        raceState = RaceState.Finished;
        tempoCorrida = Time.time - _timeWhenGunFired;
        RaceManager manager = FindObjectOfType<RaceManager>();
        if (manager != null) manager.RegistrarChegada(this);
    }

    void ProcessarReacao()
    {
        float tempoDecorridoDesdeTiro = Time.time - _timeWhenGunFired;

        if (tempoAlvo > 0)
        {
            if (tempoDecorridoDesdeTiro >= 0.15f) 
            {
                StartRunning(0.15f);
            }
            return;
        }
        if (modoAutomatico || usarAutoClick) 
        {
            if (tempoDecorridoDesdeTiro >= tempoReacaoAuto) { 
                StartRunning(tempoReacaoAuto); 
            }
        }
        else if (!isBot) 
        {
            if (Input.GetKeyDown(KeyCode.Space)) 
            { 
                if (tempoDecorridoDesdeTiro < 0.1f) 
                {
                    Debug.Log("QUEIMOU LARGADA!");
                    raceState = RaceState.FalseStart;
                    RaceManager manager = FindObjectOfType<RaceManager>();
                    if (manager != null) manager.RegistrarQueimaLargada(this);
                }
                else
                {
                    StartRunning(tempoDecorridoDesdeTiro); 
                }
            }
        }
    }

    void StartRunning(float reactTime) {
        raceState = RaceState.Running;
        tempoReacaoReal = reactTime;
        velocidadeAtual = forcaBaseClique * forcaInicialMinima; 
    }

    void UpdateFisica()
    {
        if (tempoAlvo > 0)
        {
            float velocidadeNecessaria = (100f / tempoAlvo) * 1.02f;
            velocidadeAtual = Mathf.MoveTowards(velocidadeAtual, velocidadeNecessaria, Time.deltaTime * 8f);
            return; 
        }
        bool clicou = false;
        if (modoAutomatico) {
            float meta = (targetPace > 0) ? targetPace * ritmoAutomatico : velocidadeMaximaCap * ritmoAutomatico;
            if (stamina < 5f) meta *= 0.6f; 
            _timerAutoClick += Time.deltaTime;
            float intervalo = 1.0f / 9f;
            if (velocidadeAtual < meta && _timerAutoClick >= intervalo) {
                clicou = true;
                _timerAutoClick = 0f;
            }
        } else if (usarAutoClick) {
            _timerAutoClick += Time.deltaTime;
            if (_timerAutoClick >= (1.0f/clicksPorSegundo)) { clicou = true; _timerAutoClick = 0f; }
        } else if (!isBot) {
            clicou = Input.GetKeyDown(KeyCode.Space);
        }

        // 1. Consumo
        float consumo = staminaConsumoBase + (velocidadeAtual * staminaConsumoLinear) + (Mathf.Pow(velocidadeAtual, 3) * staminaConsumoCubico);
        if (targetPace > 0) {
            if (velocidadeAtual > targetPace) consumo += Mathf.Pow(velocidadeAtual - targetPace, 2) * staminaConsumoExcesso;
            else if (stamina < 100) stamina += (1 - (velocidadeAtual/targetPace)) * taxaRecuperacao * Time.deltaTime;
        }
        if (velocidadeAtual > 0.1f) stamina -= consumo * Time.deltaTime;
        
        // 2. Penalidades
        float arrastoFinal = arrastoBase;
        float fatorQuebra = 1f;
        if (stamina <= 0) { stamina = 0; if (targetPace > 0) { fatorQuebra = 0.5f; arrastoFinal *= 2f; } }
        
        float fatorFadiga = 1.0f;
        if (stamina < limiarFadiga) {
            float t = stamina / limiarFadiga; 
            fatorFadiga = Mathf.Lerp(penalidadeFadigaMin, 1.0f, t);
            if (fadigaQuadratica) fatorFadiga *= fatorFadiga * fatorFadiga * fatorFadiga * fatorFadiga; 
            if (fadigaDupla) fatorFadiga *= fatorFadiga * fatorFadiga; 
        }

        // 3. Ganho
        float ganho = 0f;
        float razao = velocidadeAtual / velocidadeMaximaCap;
        float dif = Mathf.Clamp01(1f - (Mathf.Pow(razao, potenciaCurvaForca) * resistenciaAoLimite));
        float inercia = (velocidadeAtual < limiarArrancada) ? Mathf.Lerp(forcaInicialMinima, 1f, velocidadeAtual/limiarArrancada) : 1f;
        if (clicou) {
            ganho = forcaBaseClique * dif * fatorFadiga * fatorQuebra * inercia;
        }

        // 4. Aplicação
        stamina = Mathf.Clamp(stamina, 0, 100);
        arrastoFinal += velocidadeAtual * arrastoDinamicoLinear;
        arrastoFinal += (velocidadeAtual * velocidadeAtual) * arrastoDinamicoQuadratico;

        if (clicou && ganho > 0) velocidadeAtual += ganho;
        
        if (velocidadeAtual > 0) {
            velocidadeAtual -= arrastoFinal * Time.deltaTime;
            velocidadeAtual = Mathf.Max(0, velocidadeAtual);
        }
        velocidadeAtual = Mathf.Clamp(velocidadeAtual, 0, velocidadeMaximaCap + 0.5f);

    }

    void UpdateMovimento() {
        if (trackManager != null) {
            distanciaPercorridaTotal += velocidadeAtual * Time.deltaTime;
            float raiaAjustada = (float)raiaAlvo + (offsetLateral / trackManager.larguraRaia);
            var dados = trackManager.CalcularRota(_offsetLargada + distanciaPercorridaTotal, raiaAjustada);
            transform.position = dados.posicao;
            transform.rotation = dados.rotacao;
        } else {
            transform.Translate(Vector3.forward * velocidadeAtual * Time.deltaTime);
            distanciaPercorridaTotal += velocidadeAtual * Time.deltaTime;
        }
    }
    
    public void PosicionarNaPista() {
        if (trackManager == null) return; 
        float raiaAjustada = (float)raiaAlvo + (offsetLateral / trackManager.larguraRaia);
        var dados = trackManager.CalcularRota(_offsetLargada, raiaAjustada);
        transform.position = dados.posicao;
        transform.rotation = dados.rotacao;

        if (provaSelecionada == TipoDeProva.Revezamento_4x100m && numeroPerna > 1)
        {
            float exchangeDistance = GetRelayExchangeDistance(numeroPerna);
            dados = trackManager.CalcularRota(exchangeDistance, raiaAjustada);
            transform.position = dados.posicao;
            transform.rotation = dados.rotacao;
        }
    }
    
    private float GetRelayExchangeDistance(int leg)
    {
        float baseStart100 = trackManager.GetPontoDeLargada(100f, 1);
        baseStart100 = (raiaAlvo-3) * 7.67f + ((100*leg)-10f); 
        return baseStart100;
    }

    void UpdateTelemetria() {
        if (!f10 && distanciaPercorridaTotal >= 10f) { t_10m = tempoCorrida; f10=true; }
        if (!f20 && distanciaPercorridaTotal >= 20f) { t_20m = tempoCorrida; f20=true; }
        if (!f30 && distanciaPercorridaTotal >= 30f) { t_30m = tempoCorrida; f30=true; }
        if (!f40 && distanciaPercorridaTotal >= 40f) { t_40m = tempoCorrida; f40=true; }
        if (!f100 && distanciaPercorridaTotal >= 100f) { t_100m = tempoCorrida; f100=true; }
        if (!f200 && distanciaPercorridaTotal >= 200f) { t_200m = tempoCorrida; f200=true; }
        if (!f400 && distanciaPercorridaTotal >= 400f) { t_400m = tempoCorrida; f400=true; }
        if (!f800 && distanciaPercorridaTotal >= 800f) { t_800m = tempoCorrida; f800=true; }
        if (!f1500 && distanciaPercorridaTotal >= 1500f) { t_1500m = tempoCorrida; f1500=true; }
    }
}