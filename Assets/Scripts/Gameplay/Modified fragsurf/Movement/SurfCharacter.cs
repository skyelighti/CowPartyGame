using UnityEngine;

using System.Collections;

using System.Collections.Generic;

using UnityEngine.Rendering;

using UnityEngine.InputSystem;

using Unity.Netcode;

using Unity.Netcode.Components;



namespace Fragsurf.Movement

{



    /// <summary>

    /// Easily add a surfable character to the scene

    /// </summary>

    [AddComponentMenu("Fragsurf/Surf Character")]

    public class SurfCharacter : NetworkBehaviour, ISurfControllable

    {
        bool isLocalPlayer;

        public enum ColliderType

        {

            Capsule,

            Box

        }



        ///// Fields /////

        [Header("For Multiplayer")]

        [SerializeField] Transform targetTransform;

        [Header("Input Mappings")]

        [SerializeField] InputActionReference jumpAction;

        [SerializeField] InputActionReference moveAction;

        [SerializeField] InputActionReference crouchAction;



        [Header("Physics Settings")]

        public Vector3 colliderSize = new Vector3(1f, 2f, 1f);

        [HideInInspector] public ColliderType collisionType { get { return ColliderType.Box; } } // Capsule doesn't work anymore; I'll have to figure out why some other time, sorry.

        public float weight = 75f;

        public float rigidbodyPushForce = 2f;

        public bool solidCollider = false;



        [Header("View Settings")]

        public Transform viewTransform;

        public Transform playerRotationTransform;



        [Header("Crouching setup")]

        public float crouchingHeightMultiplier = 0.5f;

        public float crouchingSpeed = 10f;

        float defaultHeight;

        bool allowCrouch = true; // This is separate because you shouldn't be able to toggle crouching on and off during gameplay for various reasons



        [Header("Features")]

        public bool crouchingEnabled = true;

        public bool slidingEnabled = false;

        public bool laddersEnabled = true;

        public bool supportAngledLadders = true;



        [Header("Step offset (can be buggy, enable at your own risk)")]

        public bool useStepOffset = false;

        public float stepOffset = 0.35f;



        [Header("Movement Config")]

        [SerializeField]

        public MovementConfig movementConfig;





        private GameObject _groundObject;

        private Vector3 _baseVelocity;

        private Collider _collider;

        private Vector3 _angles;

        private Vector3 _startPosition;

        private GameObject _colliderObject;

        private GameObject _cameraWaterCheckObject;

        private CameraWaterCheck _cameraWaterCheck;



        private MoveData _moveData = new MoveData();

        private SurfController _controller = new SurfController();



        private Rigidbody rb;



        private List<Collider> triggers = new List<Collider>();

        private int numberOfTriggers = 0;



        private bool underwater = false;



        ///// Properties /////



        public MoveType moveType { get { return MoveType.Walk; } }

        public MovementConfig moveConfig { get { return movementConfig; } }

        public MoveData moveData { get { return _moveData; } }

        public new Collider collider { get { return _collider; } }



        public GameObject groundObject

        {



            get { return _groundObject; }

            set { _groundObject = value; }



        }



        public Vector3 baseVelocity { get { return _baseVelocity; } }



        public Vector3 forward { get { return viewTransform.forward; } }

        public Vector3 right { get { return viewTransform.right; } }

        public Vector3 up { get { return viewTransform.up; } }



        Vector3 prevPosition;

        private NetAnimManager netAnim;

        public bool ignorePlayerInput; // Christopher added this to implement stunning

        ///// Methods /////



        private void OnDrawGizmos()

        {

            Gizmos.color = Color.red;

            Gizmos.DrawWireCube(targetTransform.position, colliderSize);

        }



        private void Awake()

        {



            _controller.playerTransform = playerRotationTransform;



            if (viewTransform != null)

            {



                _controller.camera = viewTransform;

                _controller.cameraYPos = viewTransform.localPosition.y;



            }



        }

        public override void OnNetworkSpawn()
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            if (networkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("Is local player");
                isLocalPlayer = true;
            }
            else
            {
                Debug.Log("Is not local player");
            }
        }

        private void Start()
        {
            if (viewTransform == null)

                viewTransform = Camera.main.transform;



            if (playerRotationTransform == null && transform.childCount > 0)

                playerRotationTransform = transform.GetChild(0);



            if (playerRotationTransform == null && transform.childCount <= 0)

                playerRotationTransform = transform;



            if (targetTransform == null)

                targetTransform = transform;





            _controller.playerTransform = playerRotationTransform; // Checks Again lol 



            CreateCollider();



            // Water check

            _cameraWaterCheckObject = new GameObject("Camera water check");

            _cameraWaterCheckObject.layer = gameObject.layer;

            _cameraWaterCheckObject.transform.position = viewTransform.position;



            SphereCollider _cameraWaterCheckSphere = _cameraWaterCheckObject.AddComponent<SphereCollider>();

            _cameraWaterCheckSphere.radius = 0.1f;

            _cameraWaterCheckSphere.isTrigger = true;



            Rigidbody _cameraWaterCheckRb = _cameraWaterCheckObject.AddComponent<Rigidbody>();

            _cameraWaterCheckRb.useGravity = false;

            //_cameraWaterCheckRb.isKinematic = true;



            _cameraWaterCheck = _cameraWaterCheckObject.AddComponent<CameraWaterCheck>();



            prevPosition = targetTransform.position;



            _collider = gameObject.GetComponent<Collider>();



            if (_collider != null)

                GameObject.Destroy(_collider);



            // rigidbody is required to collide with triggers

            rb = gameObject.GetComponent<Rigidbody>();

            if (rb == null)

                rb = gameObject.AddComponent<Rigidbody>();



            allowCrouch = crouchingEnabled;



            //rb.isKinematic = true;

            rb.useGravity = false;

            rb.angularDamping = 0f;

            rb.linearDamping = 0f;

            rb.mass = weight;





            switch (collisionType)

            {



                // Box collider

                case ColliderType.Box:



                    _collider = _colliderObject.AddComponent<BoxCollider>();



                    var boxc = (BoxCollider)_collider;

                    boxc.size = colliderSize;



                    defaultHeight = boxc.size.y;



                    break;



                // Capsule collider

                case ColliderType.Capsule:



                    _collider = _colliderObject.AddComponent<CapsuleCollider>();



                    var capc = (CapsuleCollider)_collider;

                    capc.height = colliderSize.y;

                    capc.radius = colliderSize.x / 2f;



                    defaultHeight = capc.height;



                    break;



            }



            _moveData.slopeLimit = movementConfig.slopeLimit;



            _moveData.rigidbodyPushForce = rigidbodyPushForce;



            _moveData.slidingEnabled = slidingEnabled;

            _moveData.laddersEnabled = laddersEnabled;

            _moveData.angledLaddersEnabled = supportAngledLadders;



            _moveData.playerTransform = targetTransform;

            _moveData.viewTransform = viewTransform;

            _moveData.viewTransformDefaultLocalPos = viewTransform.localPosition;



            _moveData.defaultHeight = defaultHeight;

            _moveData.crouchingHeight = crouchingHeightMultiplier;

            _moveData.crouchingSpeed = crouchingSpeed;



            _collider.isTrigger = !solidCollider;

            _moveData.origin = targetTransform.position;

            _startPosition = targetTransform.position;



            _moveData.useStepOffset = useStepOffset;

            _moveData.stepOffset = stepOffset;



        }

        public void CreateCollider()
        {
            _colliderObject = new GameObject("PlayerCollider");

            _colliderObject.layer = gameObject.layer;

            _colliderObject.transform.SetParent(targetTransform);

            _colliderObject.transform.rotation = Quaternion.identity;

            _colliderObject.transform.localPosition = Vector3.zero;

            _colliderObject.transform.SetSiblingIndex(0);
        }

        private void Update()

        {

            //anim = GetComponentInChildren<Animator>();

            _colliderObject.transform.rotation = Quaternion.identity;



            //UpdateTestBinds ();

            UpdateMoveData();



            transform.rotation = Quaternion.Euler(new Vector3(0, _moveData.viewTransform.rotation.eulerAngles.y, 0));



            // Previous movement code

            Vector3 positionalMovement = targetTransform.position - prevPosition;

            targetTransform.position = prevPosition;

            moveData.origin += positionalMovement;//--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------



            // Triggers

            if (numberOfTriggers != triggers.Count)

            {

                numberOfTriggers = triggers.Count;



                underwater = false;

                triggers.RemoveAll(item => item == null);

                foreach (Collider trigger in triggers)

                {



                    if (trigger == null)

                        continue;



                    if (trigger.GetComponentInParent<Water>())

                        underwater = true;



                }



            }



            _moveData.cameraUnderwater = _cameraWaterCheck.IsUnderwater();

            _cameraWaterCheckObject.transform.position = viewTransform.position;

            moveData.underwater = underwater;



            if (allowCrouch)

                _controller.Crouch(this, movementConfig, Time.deltaTime);





            _controller.ProcessMovement(this, movementConfig, Time.deltaTime);// is playerTransform set?



            targetTransform.position = moveData.origin;//--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------/--------------

            prevPosition = targetTransform.position;



            _colliderObject.transform.rotation = Quaternion.identity;



            //INEFFICIENT: CHANGE LATER

            /*

            Vector3 storedPos = transform.position;

            transform.position = Vector3.zero;

            transform.parent.position = storedPos;

            */


            if (isLocalPlayer)
            {
                if (moveData.velocity.magnitude > 10f)
                {
                    CamFXHandler.Instance.EnableSpeedLines();
                }
                else
                {
                    CamFXHandler.Instance.DisableSpeedLines();
                }
            }
        }



        private void UpdateTestBinds()

        {



            if (Input.GetKeyDown(KeyCode.Backspace))

                //ResetPosition ();

                print("reset works");



        }



        private void ResetPosition()

        {



            moveData.velocity = Vector3.zero;

            moveData.origin = _startPosition;



        }



        private void UpdateMoveData()

        {

            // Christopher made this line into a ternary statement that checks if player input should be ignored.
            // It was:
            // Vector2 moveAxis = moveAction.action.ReadValue<Vector2>();
            Vector2 moveAxis = ignorePlayerInput ? Vector2.zero : moveAction.action.ReadValue<Vector2>();



            _moveData.verticalAxis = moveAxis.y; //Input.GetAxisRaw ("Vertical");

            _moveData.horizontalAxis = moveAxis.x; //Input.GetAxisRaw ("Horizontal");



            //  _moveData.sprinting = Input.GetButton ("Sprint");



            bool isCrouching = crouchAction.action.ReadValue<bool>();

            _moveData.crouching = isCrouching;



            bool moveLeft = _moveData.horizontalAxis < 0f;

            bool moveRight = _moveData.horizontalAxis > 0f;

            bool moveFwd = _moveData.verticalAxis > 0f;

            bool moveBack = _moveData.verticalAxis < 0f;



            if (!moveLeft && !moveRight)

                _moveData.sideMove = 0f;

            else if (moveLeft)

                _moveData.sideMove = -moveConfig.acceleration;

            else if (moveRight)

                _moveData.sideMove = moveConfig.acceleration;



            if (!moveFwd && !moveBack)

                _moveData.forwardMove = 0f;

            else if (moveFwd)

                _moveData.forwardMove = moveConfig.acceleration;

            else if (moveBack)

                _moveData.forwardMove = -moveConfig.acceleration;





            if (jumpAction.action.ReadValue<float>() < 0.0001f)

                _moveData.wishJump = false;

            _moveData.viewAngles = _angles;



            //Tie To Animator

            //if (anim != null)

            //{

            //    Debug.Log(_moveData.velocity.magnitude);

            netAnim= gameObject.GetComponent<NetAnimManager>();

            if (netAnim != null && gameObject.GetComponent<NetworkObject>().IsOwner)

            {

                if (_moveData.velocity.magnitude > 0.1 && netAnim.isWalking.Value == false)

                {

                    netAnim.isWalking.Value = true;

                }

                if (_moveData.velocity.magnitude < 0.1 && netAnim.isWalking.Value == true)

                {

                    netAnim.isWalking.Value = false;

                }

            }

            //}



        }



        public void OnJump()

        {

            _moveData.wishJump = true;

            netAnim= gameObject.GetComponent<NetAnimManager>();

            if (netAnim != null && gameObject.GetComponent<NetworkObject>().IsOwner)

            {


                netAnim.isJumping.Value = true;

                StartCoroutine(JumpDuration());

            }

        }



        IEnumerator JumpDuration()

        {

            

            yield return new WaitForSeconds(0.5f);

            netAnim.isJumping.Value = false;



        }

        private void DisableInput()

        {



            _moveData.verticalAxis = 0f;

            _moveData.horizontalAxis = 0f;

            _moveData.sideMove = 0f;

            _moveData.forwardMove = 0f;

            _moveData.wishJump = false;

            netAnim = gameObject.GetComponent<NetAnimManager>();

            if (netAnim != null && gameObject.GetComponent<NetworkObject>().IsOwner)

            {

                netAnim.isJumping.Value = false;

            }



        }

        public void ApplyImpulse(Vector3 direction, float force)

        {

            _moveData.velocity += direction.normalized * force;

        }



        /// <summary>

        /// 

        /// </summary>

        /// <param name="angle"></param>

        /// <param name="from"></param>

        /// <param name="to"></param>

        /// <returns></returns>

        public static float ClampAngle(float angle, float from, float to)

        {



            if (angle < 0f)

                angle = 360 + angle;



            if (angle > 180f)

                return Mathf.Max(angle, 360 + from);



            return Mathf.Min(angle, to);



        }



        private void OnTriggerEnter(Collider other)

        {



            if (!triggers.Contains(other))

                triggers.Add(other);



        }



        private void OnTriggerExit(Collider other)

        {



            if (triggers.Contains(other))

                triggers.Remove(other);



        }



        private void OnCollisionStay(Collision collision)

        {



            if (collision.rigidbody == null)

                return;



            Vector3 relativeVelocity = collision.relativeVelocity * collision.rigidbody.mass / 50f;

            Vector3 impactVelocity = new Vector3(relativeVelocity.x * 0.0025f, relativeVelocity.y * 0.00025f, relativeVelocity.z * 0.0025f);



            float maxYVel = Mathf.Max(moveData.velocity.y, 10f);

            Vector3 newVelocity = new Vector3(moveData.velocity.x + impactVelocity.x, Mathf.Clamp(moveData.velocity.y + Mathf.Clamp(impactVelocity.y, -0.5f, 0.5f), -maxYVel, maxYVel), moveData.velocity.z + impactVelocity.z);



            newVelocity = Vector3.ClampMagnitude(newVelocity, Mathf.Max(moveData.velocity.magnitude, 30f));

            moveData.velocity = newVelocity;



        }


        /*
        void OnAttack(InputValue inputValue)

        {

            Cursor.lockState = CursorLockMode.Locked;

        }

        void OnEsc(InputValue inputValue)

        {

            Cursor.lockState = CursorLockMode.None;

        }
        */


    }

}



