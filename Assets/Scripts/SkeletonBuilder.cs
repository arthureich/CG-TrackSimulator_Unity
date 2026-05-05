using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SkeletonBuilder : MonoBehaviour
{
    [Header("Cores")]
    public Color corPlayer = Color.cyan; 
    public Color corNPC = Color.red;
    [Header("Configurações do Esqueleto (Escala 1.85m)")]
    public float larguraQuadril = 0.35f; 
    public float comprimentoCoxa = 0.50f; 
    public float comprimentoCanela = 0.50f; 
    public float comprimentoTronco = 0.55f;
    public float larguraOmbros = 0.45f;
    public float comprimentoBraco = 0.35f; 
    public float comprimentoAntebraco = 0.30f; 
    public float tamanhoCabeca = 0.23f;

    [HideInInspector] public Transform pelvis, spine, head; 
    [HideInInspector] public Transform hipRight, kneeRight, ankleRight;
    [HideInInspector] public Transform hipLeft, kneeLeft, ankleLeft;
    [HideInInspector] public Transform shoulderRight, elbowRight, handRight;
    [HideInInspector] public Transform shoulderLeft, elbowLeft, handLeft;

    // Cores fixas do padrão solicitado
    private Color corCorpo = new Color(0.7f, 0.75f, 0.8f);
    private Color corPernaSup = Color.black;

    void Awake() { BuildSkeleton(); }

    public void Start()
    {
        var racer = GetComponent<SkeletonRacer>();
        if (racer != null && racer.corForcada != Color.clear)
        {
            if (racer.isBot) corNPC = racer.corForcada; 
            else corPlayer = racer.corForcada;
        }
        BuildSkeleton();
    }

    void OnValidate()
    {
        #if UNITY_EDITOR
        EditorApplication.delayCall += () => { if (this != null && gameObject != null) BuildSkeleton(); };
        #endif
    }

    public void BuildSkeleton()
    {
        Color corPrincipal = corPlayer;
        SkeletonRacer racer = GetComponent<SkeletonRacer>();
        if (racer.isBot) corPrincipal = corNPC;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
        }

        float alturaInicial = comprimentoCoxa + comprimentoCanela + 0.1f;
        
        // --- 1. PÉLVIS & TRONCO ---
        pelvis = CreateJoint("Pelvis", this.transform);
        pelvis.localPosition = new Vector3(0, alturaInicial, 0);
        CreateBoneVisual(pelvis, 0.15f, Color.black, new Vector3(larguraQuadril, 0.2f, 0.2f), Vector3.zero);

        spine = CreateJoint("Spine", pelvis);
        CreateBoneVisual(spine, comprimentoTronco, corPrincipal, new Vector3(0.27f, comprimentoTronco/2, 0.17f), new Vector3(0, comprimentoTronco/2, 0));

        head = CreateJoint("Head", spine);
        head.localPosition = new Vector3(0, comprimentoTronco, 0);
        CreateHeadVisual(head);

        // --- 2. PERNAS ---
        // Direita
        hipRight = CreateJoint("Hip_Right", pelvis);
        hipRight.localPosition = new Vector3(larguraQuadril / 2, 0, 0);
        CreateArticulacao(hipRight, 0.12f, Color.black);
        CreateBoneVisual(hipRight, comprimentoCoxa, corPernaSup, Vector3.zero, new Vector3(0, -comprimentoCoxa/2, 0));

        kneeRight = CreateJoint("Knee_Right", hipRight);
        kneeRight.localPosition = new Vector3(0, -comprimentoCoxa, 0);
        CreateArticulacao(kneeRight, 0.11f, corCorpo);
        CreateBoneVisual(kneeRight, comprimentoCanela, corCorpo, Vector3.zero, new Vector3(0, -comprimentoCanela/2, 0));

        ankleRight = CreateJoint("Ankle_Right", kneeRight);
        ankleRight.localPosition = new Vector3(0, -comprimentoCanela, 0);
        CreateFootVisual(ankleRight);

        // Esquerda
        hipLeft = CreateJoint("Hip_Left", pelvis);
        hipLeft.localPosition = new Vector3(-larguraQuadril / 2, 0, 0);
        CreateArticulacao(hipLeft, 0.12f, Color.black);
        CreateBoneVisual(hipLeft, comprimentoCoxa, corPernaSup, Vector3.zero, new Vector3(0, -comprimentoCoxa/2, 0));

        kneeLeft = CreateJoint("Knee_Left", hipLeft);
        kneeLeft.localPosition = new Vector3(0, -comprimentoCoxa, 0);
        CreateArticulacao(kneeLeft, 0.11f, corCorpo);
        CreateBoneVisual(kneeLeft, comprimentoCanela, corCorpo, Vector3.zero, new Vector3(0, -comprimentoCanela/2, 0));

        ankleLeft = CreateJoint("Ankle_Left", kneeLeft);
        ankleLeft.localPosition = new Vector3(0, -comprimentoCanela, 0);
        CreateFootVisual(ankleLeft);

        // --- 3. BRAÇOS ---
        // Direito
        shoulderRight = CreateJoint("Shoulder_Right", spine);
        shoulderRight.localPosition = new Vector3(-larguraOmbros / 2, comprimentoTronco - 0.05f, 0);
        CreateArticulacao(shoulderRight, 0.11f, corCorpo);
        CreateBoneVisual(shoulderRight, comprimentoBraco, corCorpo, Vector3.zero, new Vector3(0, -comprimentoBraco/2, 0));

        elbowRight = CreateJoint("Elbow_Right", shoulderRight);
        elbowRight.localPosition = new Vector3(0, -comprimentoBraco, 0);
        CreateArticulacao(elbowRight, 0.09f, corCorpo);
        CreateBoneVisual(elbowRight, comprimentoAntebraco, corCorpo, Vector3.zero, new Vector3(0, -comprimentoAntebraco/2, 0));

        handRight = CreateJoint("Hand_Right", elbowRight);
        handRight.localPosition = new Vector3(0, -comprimentoAntebraco, 0);
        CreateHandVisual(handRight);

        // Esquerdo
        shoulderLeft = CreateJoint("Shoulder_Left", spine);
        shoulderLeft.localPosition = new Vector3(larguraOmbros / 2, comprimentoTronco - 0.05f, 0);
        CreateArticulacao(shoulderLeft, 0.11f, corCorpo);
        CreateBoneVisual(shoulderLeft, comprimentoBraco, corCorpo, Vector3.zero, new Vector3(0, -comprimentoBraco/2, 0));

        elbowLeft = CreateJoint("Elbow_Left", shoulderLeft);
        elbowLeft.localPosition = new Vector3(0, -comprimentoBraco, 0);
        CreateArticulacao(elbowLeft, 0.09f, corCorpo);
        CreateBoneVisual(elbowLeft, comprimentoAntebraco, corCorpo, Vector3.zero, new Vector3(0, -comprimentoAntebraco/2, 0));

        handLeft = CreateJoint("Hand_Left", elbowLeft);
        handLeft.localPosition = new Vector3(0, -comprimentoAntebraco, 0);
        CreateHandVisual(handLeft);
    }

    Transform CreateJoint(string name, Transform parent)
    {
        GameObject joint = new GameObject(name);
        joint.transform.parent = parent;
        joint.transform.localPosition = Vector3.zero;
        joint.transform.localRotation = Quaternion.identity;
        return joint.transform;
    }

    void CreateArticulacao(Transform parent, float scale, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Joint_Visual";
        sphere.transform.parent = parent;
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.one * scale;
        var col = sphere.GetComponent<Collider>();
        if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
        ApplyColorSafe(sphere.GetComponent<Renderer>(), color);
    }

    void CreateBoneVisual(Transform parentJoint, float length, Color color, Vector3 customScale, Vector3 offset)
    {
        GameObject bone = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bone.name = "Geometry";
        bone.transform.parent = parentJoint;
        var col = bone.GetComponent<Collider>();
        if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
        if (customScale == Vector3.zero) bone.transform.localScale = new Vector3(0.10f, length / 2, 0.10f);
        else bone.transform.localScale = customScale;
        bone.transform.localPosition = offset;
        ApplyColorSafe(bone.GetComponent<Renderer>(), color);
    }

    void CreateFootVisual(Transform parentJoint)
    {
        GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        foot.name = "Foot";
        foot.transform.parent = parentJoint;
        DestroyCol(foot);
        foot.transform.localScale = new Vector3(0.09f, 0.08f, 0.25f);
        foot.transform.localPosition = new Vector3(0, -0.05f, 0.05f); 
        ApplyColorSafe(foot.GetComponent<Renderer>(), Color.black);
    }

    void CreateHandVisual(Transform parentJoint)
    {
        GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hand.name = "Hand";
        hand.transform.parent = parentJoint;
        DestroyCol(hand);
        hand.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        hand.transform.localPosition = new Vector3(0, -0.05f, 0); 
        ApplyColorSafe(hand.GetComponent<Renderer>(), corCorpo);
    }

    void CreateHeadVisual(Transform parentJoint)
    {
        GameObject headGeo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headGeo.name = "HeadGeo";
        headGeo.transform.parent = parentJoint;
        DestroyCol(headGeo);
        headGeo.transform.localScale = new Vector3(0.2f, 0.25f, 0.22f);
        headGeo.transform.localPosition = new Vector3(0, 0.125f, 0);
        ApplyColorSafe(headGeo.GetComponent<Renderer>(), corCorpo);
        
        GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visor.transform.parent = headGeo.transform;
        DestroyCol(visor);
        visor.transform.localScale = new Vector3(0.8f, 0.3f, 0.2f);
        visor.transform.localPosition = new Vector3(0, 0.1f, 0.45f); 
        ApplyColorSafe(visor.GetComponent<Renderer>(), Color.black);
    }

    void ApplyColorSafe(Renderer rend, Color color)
    {
        if (Application.isPlaying) rend.material.color = color;
        else {
            Material temp = new Material(Shader.Find("Standard")); 
            temp.color = color; rend.sharedMaterial = temp;
        }
    }

    void DestroyCol(GameObject obj) {
        var col = obj.GetComponent<Collider>();
        if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);
    }
}