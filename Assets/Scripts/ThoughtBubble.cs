using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtBubble : MonoBehaviour
{
    public enum Thought
    {
        Nohting,
        Food,
        Water,
        Rest,
        Happy,
        Hurt,
    }

    [System.Serializable]
    struct ThoughtBubbleIcons
    {
        public Thought thought;
        public Sprite sprite;
    }

    [SerializeField]
    Image bubbleImage;


    [SerializeField]
    Image iconImage;

    [SerializeField] bool showStrikes;
    [SerializeField] Image strikeOne;
    [SerializeField] Image strikeTwo;
    [SerializeField] Image strikeThree;

    [SerializeField] Sprite normalBubble;
    [SerializeField] Sprite dangerBubble;

    [SerializeField]
    List<ThoughtBubbleIcons> icons;

    Sheep sheep;

    private void Start()
    {
        sheep = GetComponentInParent<Sheep>();

        // Ensure they are all disabled
        strikeOne?.gameObject.SetActive(false);
        strikeTwo?.gameObject.SetActive(false);
        strikeThree?.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (sheep != null)
            bubbleImage.sprite = sheep.ResourcesLow ? dangerBubble : normalBubble;

        if (!showStrikes)
            return;

        //strikeOne?.gameObject.SetActive(sheep.Strikes > 0);
        //strikeTwo?.gameObject.SetActive(sheep.Strikes > 1);
        //strikeThree?.gameObject.SetActive(sheep.Strikes > 2);
    }

    public void SetThought(Thought thought)
    {
        bubbleImage.gameObject.SetActive(true);
        iconImage.sprite = icons.Where(i => i.thought == thought)
                                .Select(s => s.sprite)
                                .FirstOrDefault();
    }

    public void DisableThought()
    {
        bubbleImage.gameObject.SetActive(false);
    }
}