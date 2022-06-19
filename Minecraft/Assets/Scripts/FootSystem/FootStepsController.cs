using UnityEngine;

public class FootStepsController : MonoBehaviour
{
    public WorldSupervisor World;

    private AudioSource _audioSource;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Step()
    {
        AudioClip clip = GetRandomClip();
        if (clip != null)
            _audioSource.PlayOneShot(clip);
    }

    private AudioClip GetRandomClip()
    {
        Vector3 pos = transform.position + Vector3.down;
        byte blockTypeIndex = World.GetTerrainFromGlobalCoord(pos).GetBlockTypeFromGlobalCoord(pos);

        AudioClip sound = World.BlockTypes[blockTypeIndex].FootSound;
        
        return sound;
    }
}