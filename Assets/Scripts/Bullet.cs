using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float power;
    public ParticleSystem hitVFX;
    public CustomCharacterController characterController;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Venom") || collision.collider.CompareTag("Character"))
        {
            var player = collision.collider.GetComponentInChildren<PlayerData>();

            if (player)
            {
                var died = player.Damage(power);

                if(died)
                {
                    characterController.Target = null;
                    characterController.OverrideRotation = false;
                }

                if (hitVFX)
                {
                    var ptcRenderer = Instantiate(hitVFX, collision.GetContact(0).point, Quaternion.LookRotation(collision.GetContact(0).normal), null).GetComponent<Renderer>();

                    var particleMat = new Material(ptcRenderer.sharedMaterial);
                    particleMat.SetColor("_BaseColor", player.colors.mainColor);
                    ptcRenderer.material = particleMat;
                }

            }
        }

        Destroy(gameObject);
    }
}