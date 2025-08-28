using GridSelector;
using UnityEngine;
public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private Selector _selector;

    [SerializeField] private AudioClip _grabItemSFX;
    [SerializeField] private AudioClip _dropItemSFX;
    [SerializeField] private AudioClip _navigateSFX;
    [SerializeField] private AudioClip _navigateItemSFX;
    private void Awake()
    {
        _selector.GrabItemEvent += OnItemGrab;
        _selector.DropItemEvent += OnItemDrop;
        _selector.NavigateEvent += OnNavigate;
        _selector.NavigateWithItemEvent += OnNavigateWithItem;
    }

    private void Start()
    {
        _musicSource.Play();
    }
    private void OnDisable()
    {
        _selector.GrabItemEvent -= OnItemGrab;
        _selector.DropItemEvent -= OnItemDrop;
        _selector.NavigateEvent -= OnNavigate;
        _selector.NavigateWithItemEvent -= OnNavigateWithItem;
    }

    private void OnItemGrab()
    {
        _sfxSource.PlayOneShot(_grabItemSFX);
    }

    private void OnItemDrop()
    {
        _sfxSource.PlayOneShot(_dropItemSFX);
    }

    private void OnNavigate()
    {
        _sfxSource.PlayOneShot(_navigateSFX);
    }

    private void OnNavigateWithItem()
    {
        _sfxSource.PlayOneShot(_navigateItemSFX);
    }

}
