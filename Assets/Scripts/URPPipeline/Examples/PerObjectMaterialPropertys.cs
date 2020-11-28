using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialPropertys : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");
    static int cutoffId = Shader.PropertyToID("_Cutoff"),
        metallicId = Shader.PropertyToID("_Metallic"),
        smoothnessId = Shader.PropertyToID("_Smoothness"),
        emissionColorId=Shader.PropertyToID("_EmissionColor"); //自发光

    static MaterialPropertyBlock block;

    [SerializeField]
    Color baseColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f,metallic=0f,smoothness=0.5f;
    [SerializeField, ColorUsage(false, true)]
    Color emissionColor = Color.black;
    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (block == null)
            block = new MaterialPropertyBlock();
        block.SetColor(baseColorId,baseColor);
        block.SetFloat(cutoffId, cutoff);
        block.SetFloat(metallicId, metallic);
        block.SetFloat(smoothnessId, smoothness);
        block.SetColor(emissionColorId,emissionColor);
        //block.Clear();
        GetComponent<Renderer>().SetPropertyBlock(block);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            block.Clear();
            GetComponent<Renderer>().SetPropertyBlock(block);
        }
    }
}
