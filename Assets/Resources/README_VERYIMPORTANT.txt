WARNING: Always make sure prefabs that are supposed to be instantiated over the network are within a Resources folder, this is a Photon requirement.
This requirement is mainly because Photon uses the class "Resources" of UnityEngine to find the proper object that must be instantiated and loaded
over the network.
Read more documentation in: https://doc.photonengine.com/en-us/pun/v2/demos-and-tutorials/pun-basics-tutorial/player-instantiation
And: https://docs.unity3d.com/ScriptReference/Resources.html