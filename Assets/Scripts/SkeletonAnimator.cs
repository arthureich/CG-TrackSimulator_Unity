using UnityEngine;

public class SkeletonAnimator : MonoBehaviour
{
    [Header("Referências")]
    public SkeletonRacer racer; // O Cérebro
    public SkeletonBuilder builder; // Os Ossos

    [Header("Biomecânica Visual")]
    public float frequencyMultiplier = 1.15f; 
    public float armAmplitude = 60f; 
    public float elbowBaseAngle = 90f;
    public float bounceAmplitude = 0.1f;
    [Header("Frenagem")]
    public float leanAngleBrake = 10f;
    
    [Header("Transição de Largada (Drive Phase)")]
    [Tooltip("Inclinação máxima no começo (graus).")]
    public float leanAngleStart = 50f; 
    [Tooltip("Inclinação final em alta velocidade.")]
    public float leanAngleCruise = 5f;
    [Tooltip("Velocidade onde ele fica totalmente em pé.")]
    public float speedForUpright = 6.5f; 
    [Tooltip("Altura do quadril no bloco (0 a 1).")]
    public float blockHeightFactor = 0.65f;

    private float _ciclo = 0f;
    private float _pelvisBaseHeight = 0f;

    void Start()
    {
        if (!racer) racer = GetComponent<SkeletonRacer>();
        if (!builder) builder = GetComponent<SkeletonBuilder>();
        
        if (builder) {
            _pelvisBaseHeight = builder.comprimentoCoxa + builder.comprimentoCanela;
            if (_pelvisBaseHeight < 0.1f) _pelvisBaseHeight = 1.0f; 
        }
    }

    void Update()
    {
        if (!racer || !builder) return;

        // --- MÁQUINA DE ESTADOS VISUAL ---
        switch (racer.raceState)
        {
            case SkeletonRacer.RaceState.Ready:
                if (DeveUsarBloco()) PoseNoBloco();
                PoseEmPe();
                _ciclo = 0;
                break;

            case SkeletonRacer.RaceState.Set:
            case SkeletonRacer.RaceState.GunFired: 
                if (IsSprint(racer.provaSelecionada) || racer.provaSelecionada == SkeletonRacer.TipoDeProva.Revezamento_4x100m ) PoseNoBloco();
                else PoseEmPe();
                _ciclo = 0;
                break;

            case SkeletonRacer.RaceState.Running:
                AnimarCorrida();
                break;
            case SkeletonRacer.RaceState.Finished:
             if (racer.velocidadeAtual > 0.1f) AnimarFrenagem();
             else PoseEmPe(); 
             break;
        }
    }

    bool DeveUsarBloco() {
        if (IsSprint(racer.provaSelecionada)) return true;
        if (racer.provaSelecionada == SkeletonRacer.TipoDeProva.Revezamento_4x100m && racer.numeroPerna == 1) return true;
        return false;
    }

    bool IsSprint(SkeletonRacer.TipoDeProva prova) {
        return (prova == SkeletonRacer.TipoDeProva.Sprint_100m || 
                prova == SkeletonRacer.TipoDeProva.Sprint_200m || 
                prova == SkeletonRacer.TipoDeProva.SprintLongo_400m);
    }

    void AnimarFrenagem() {
        AnimarCorrida(); 
        builder.spine.localRotation = Quaternion.Euler(leanAngleBrake, 180f, 0);
        if (racer.velocidadeAtual < 0f) {
            PoseEmPe();
        }
    }

    // --- POSES ESTÁTICAS ---
    void PoseEmPe() {
        builder.pelvis.localPosition = new Vector3(0, _pelvisBaseHeight, 0);
        builder.spine.localRotation = Quaternion.Euler(0, 180f, 0);
        builder.head.localRotation = Quaternion.identity;
        // Zera membros
        Rot(builder.hipRight, builder.kneeRight, builder.ankleRight, 0, 0);
        Rot(builder.hipLeft, builder.kneeLeft, builder.ankleLeft, 0, 0);
        Arm(builder.shoulderRight, builder.elbowRight, 0, 0);
        Arm(builder.shoulderLeft, builder.elbowLeft, 0, 0);
    }

    void PoseNoBloco() {
        float h = _pelvisBaseHeight * blockHeightFactor;
        builder.pelvis.localPosition = new Vector3(0, h, 0);
        // Usamos -70f (Negativo) para inclinar para FRENTE/BAIXO
        builder.spine.localRotation = Quaternion.Euler(-70f, 180f, 0);
        
        // Cabeça olha pra cima (+30) para ver a pista
        builder.head.localRotation = Quaternion.Euler(30f, 0, 0);

        // Pernas Assimetricas
        Rot(builder.hipRight, builder.kneeRight, builder.ankleRight, 60, -100);
        Rot(builder.hipLeft, builder.kneeLeft, builder.ankleLeft, -10, -20);
        
        Arm(builder.shoulderRight, builder.elbowRight, 0, 0);
        Arm(builder.shoulderLeft, builder.elbowLeft, 0, 0);
    }

    // --- ANIMAÇÃO DINÂMICA ---
    void AnimarCorrida()
    {
        float vel = racer.velocidadeAtual;
        if (vel <= 0.1f) return;

        _ciclo += Time.deltaTime * (vel * frequencyMultiplier);

        // 1. INCLINAÇÃO (Drive Phase)
        float progress = Mathf.Clamp01(vel / speedForUpright);
        float currentLean = Mathf.Lerp(leanAngleStart, leanAngleCruise, progress);

        float sway = Mathf.Sin(_ciclo) * 5f * (vel/5f); 
        // Aplicamos SINAL NEGATIVO (-currentLean) para inclinar para frente
        builder.spine.localRotation = Quaternion.Euler(-currentLean, sway + 180f, 0);
        
        // Cabeça: Se o corpo está em -50 (frente), cabeça precisa de +Rotation para olhar reto
        // Compensação positiva
        builder.head.localRotation = Quaternion.Euler(currentLean * 0.6f, -sway, 0);

        // 2. BOUNCE
        float osc = Mathf.Abs(Mathf.Sin(_ciclo));
        float fp = osc * bounceAmplitude * ((vel > 0.5f)?1:0);
        builder.pelvis.localPosition = new Vector3(0, _pelvisBaseHeight + fp, 0);

        // 3. CICLO DE PERNAS
        float c = _ciclo / (2 * Mathf.PI);
        float tDir = Mathf.Repeat(c, 1.0f);
        float tEsq = Mathf.Repeat(c + 0.5f, 1.0f);

        Vector2 d, e;
        if (racer.provaSelecionada == SkeletonRacer.TipoDeProva.Sprint_100m) {
            d = CalcAngles100(tDir); e = CalcAngles100(tEsq);
        } else if (racer.provaSelecionada == SkeletonRacer.TipoDeProva.MeioFundo_800m) {
            d = CalcAngles800(tDir); e = CalcAngles800(tEsq);
        } else {
            d = CalcAngles400(tDir); e = CalcAngles400(tEsq);
        }

        Rot(builder.hipRight, builder.kneeRight, builder.ankleRight, d.x, d.y);
        Rot(builder.hipLeft, builder.kneeLeft, builder.ankleLeft, e.x, e.y);

        // 4. BRAÇOS
        float intensity = Mathf.Clamp01(vel / 5.0f);
        float curAmp = Mathf.Lerp(10f, armAmplitude, intensity);
        Arm(builder.shoulderRight, builder.elbowRight, tEsq * 2 * Mathf.PI, curAmp);
        Arm(builder.shoulderLeft, builder.elbowLeft, tDir * 2 * Mathf.PI, curAmp);
    }

    // --- FUNÇÕES AUXILIARES ---
    void Rot(Transform h, Transform k, Transform a, float a1, float a2) {
        h.localRotation = Quaternion.Euler(-a1, 0, 0); 
        k.localRotation = Quaternion.Euler(-a2, 0, 0);
        float aa = (a2<-80)?-20f:(a2>-20?20f:Mathf.Lerp(20,-20,(a2+20)/-60f)); 
        a.localRotation = Quaternion.Euler(aa, 0, 0);
    }
    void Arm(Transform s, Transform e, float t, float a) {
        float sa = Mathf.Sin(t) * a; float fe = Mathf.Clamp(sa, 0, 50f) * 0.5f; 
        s.localRotation = Quaternion.Euler(sa, 0, 0); 
        e.localRotation = Quaternion.Euler(elbowBaseAngle + fe, 0, 0);
    }
    
    // Tabelas de Angulos
    Vector2 CalcAngles100(float t) {
        float s=1f/8f; if(t<s){float p=t/s; return new Vector2(Mathf.Lerp(25,5,p),Mathf.Lerp(-10,-25,p));}
        else if(t<s*2){float p=(t-s)/s; return new Vector2(Mathf.Lerp(5,-10,p),Mathf.Lerp(-25,-15,p));}
        else if(t<s*3){float p=(t-s*2)/s; return new Vector2(Mathf.Lerp(-10,-25,p),Mathf.Lerp(-15,0,p));}
        else if(t<s*4){float p=(t-s*3)/s; return new Vector2(Mathf.Lerp(-25,-15,p),Mathf.Lerp(0,-90,p));}
        else if(t<s*5){float p=(t-s*4)/s; return new Vector2(Mathf.Lerp(-15,45,p),Mathf.Lerp(-90,-130,p));}
        else if(t<s*6){float p=(t-s*5)/s; return new Vector2(Mathf.Lerp(45,95,p),Mathf.Lerp(-130,-120,p));}
        else if(t<s*7){float p=(t-s*6)/s; return new Vector2(Mathf.Lerp(95,75,p),Mathf.Lerp(-120,-50,p));}
        else{float p=(t-s*7)/s; return new Vector2(Mathf.Lerp(75,25,p),Mathf.Lerp(-50,-10,p));}
    }
    Vector2 CalcAngles400(float t) {
        float s=1f/8f; if(t<s){float p=t/s; return new Vector2(Mathf.Lerp(-25,-5,p),Mathf.Lerp(-5,-100,p));}
        else if(t<s*2){float p=(t-s)/s; return new Vector2(Mathf.Lerp(-5,45,p),Mathf.Lerp(-100,-130,p));}
        else if(t<s*3){float p=(t-s*2)/s; return new Vector2(Mathf.Lerp(45,88,p),Mathf.Lerp(-130,-75,p));}
        else if(t<s*4){float p=(t-s*3)/s; return new Vector2(Mathf.Lerp(88,55,p),Mathf.Lerp(-75,-35,p));}
        else if(t<s*5){float p=(t-s*4)/s; return new Vector2(Mathf.Lerp(55,25,p),Mathf.Lerp(-35,-10,p));}
        else if(t<s*6){float p=(t-s*5)/s; return new Vector2(Mathf.Lerp(25,0,p),Mathf.Lerp(-10,-20,p));}
        else if(t<s*7){float p=(t-s*6)/s; return new Vector2(Mathf.Lerp(0,-20,p),Mathf.Lerp(-20,-5,p));}
        else{float p=(t-s*7)/s; return new Vector2(Mathf.Lerp(-20,-25,p),Mathf.Lerp(-5,-5,p));}
    }
    Vector2 CalcAngles800(float t) {
        float s=1f/8f; if(t<s){float p=t/s; return new Vector2(Mathf.Lerp(-20,-5,p),Mathf.Lerp(-10,-95,p));}
        else if(t<s*2){float p=(t-s)/s; return new Vector2(Mathf.Lerp(-5,35,p),Mathf.Lerp(-95,-125,p));}
        else if(t<s*3){float p=(t-s*2)/s; return new Vector2(Mathf.Lerp(35,75,p),Mathf.Lerp(-125,-80,p));}
        else if(t<s*4){float p=(t-s*3)/s; return new Vector2(Mathf.Lerp(75,50,p),Mathf.Lerp(-80,-40,p));}
        else if(t<s*5){float p=(t-s*4)/s; return new Vector2(Mathf.Lerp(50,28,p),Mathf.Lerp(-40,-10,p));}
        else if(t<s*6){float p=(t-s*5)/s; return new Vector2(Mathf.Lerp(28,5,p),Mathf.Lerp(-10,-25,p));}
        else if(t<s*7){float p=(t-s*6)/s; return new Vector2(Mathf.Lerp(5,-15,p),Mathf.Lerp(-25,-10,p));}
        else{float p=(t-s*7)/s; return new Vector2(Mathf.Lerp(-15,-20,p),Mathf.Lerp(-10,-10,p));}
    }
}