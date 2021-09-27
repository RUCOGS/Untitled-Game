using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

/// <summary>
/// Teleports from this portal position to a transform in the world (Can be another portal position)
/// </summary>
public class PortalInteractable : WorldItemEffector, IManualInteract
{
	public Transform To;
	public Sprite ClosedPortal;
	public Sprite OpenPortal;

	public GameObject PointLight;
	public GameObject SpriteLight;

	[Tooltip("This is fake, and will only activate the necessary thing")]
	public bool IsFakePortal;

	public bool ForceClosed = false;

	public void Awake() {
		UpdatePortal();
	}

	public void UpdatePortal() {
		if ((To == null && !IsFakePortal) || ForceClosed) {
			this.GetComponent<SpriteRenderer>().sprite = ClosedPortal;
			PointLight.SetActive(false);
			SpriteLight.SetActive(false);
			this.canBeManipulated = false;
		} else {
			this.GetComponent<SpriteRenderer>().sprite = OpenPortal;
			PointLight.SetActive(true);
			SpriteLight.SetActive(true);
			this.canBeManipulated = true;
		}
	}

	public void EnablePortal() {
		ForceClosed = false;
		UpdatePortal();
	}

	public override void Use() {
		base.Use();

		if(To == null) {
			return;
		}

		Player.instance.Teleport(To.position);
	}
}
