using UnityEngine;

public class ChangeMixerOnTrigger : MonoBehaviour
{
    [SerializeField] private MusicManager.SnapshotMode mixerToFadeInto = MusicManager.SnapshotMode.Alternate;
    [SerializeField] private bool switched;

    private void OnTriggerExit(Collider other)
    {
        
        if (!other.CompareTag("Player"))
        {
            return;
        }
        if (MusicManager.Instance == null)
        {
            return;
        }
        if (!switched)
        {
            switched = true;
            MusicManager.Instance.TransitionToSnapshot(mixerToFadeInto);
        }
        else
        {
            switched = false;
            MusicManager.Instance.TransitionToSnapshot(MusicManager.SnapshotMode.Normal);
        }
    }
}