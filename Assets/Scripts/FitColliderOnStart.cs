using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FitColliderOnStart : MonoBehaviour
{
    void Awake()
    {
        AjustarCollider();
    }

    void AjustarCollider()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        // Incluimos SkinnedMeshRenderer por si algunos de tus modelos lo usan.
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds totalBounds = renderers[0].bounds;

            // Empezamos desde el segundo renderer si existe
            for (int i = 1; i < renderers.Length; i++)
            {
                totalBounds.Encapsulate(renderers[i].bounds);
            }

            collider.center = transform.InverseTransformPoint(totalBounds.center);

            // Esta es la forma más segura de calcular el tamaño local a partir del tamaño global,
            // y nos aseguramos de que cada componente sea positivo con Mathf.Abs.
            Vector3 localSize = transform.InverseTransformVector(totalBounds.size);
            collider.size = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
        }
    }
}