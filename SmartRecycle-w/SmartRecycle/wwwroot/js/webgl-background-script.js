// ==========================================
// 🎨 WebGL Animated Background
// For Machine Pages - Green & White Theme
// ==========================================

function initWebGLBackground() {
    const container = document.getElementById('canvas-container');
    if (!container) return;

    // Three.js Scene Setup
    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(
        75,
        window.innerWidth / window.innerHeight,
        0.1,
        1000
    );
    const renderer = new THREE.WebGLRenderer({
        alpha: true,
        antialias: true
    });

    renderer.setSize(window.innerWidth, window.innerHeight);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    container.appendChild(renderer.domElement);

    // Create Floating Geometric Shapes
    const shapes = [];
    const geometries = [
        new THREE.IcosahedronGeometry(1, 0),
        new THREE.OctahedronGeometry(1, 0),
        new THREE.TetrahedronGeometry(1, 0),
        new THREE.TorusGeometry(0.7, 0.3, 16, 100)
    ];

    for (let i = 0; i < 20; i++) {
        const geometry = geometries[Math.floor(Math.random() * geometries.length)];
        const material = new THREE.MeshPhongMaterial({
            color: i % 2 === 0 ? 0x2ecc71 : 0xffffff,
            wireframe: true,
            transparent: true,
            opacity: 0.4 + Math.random() * 0.3
        });

        const mesh = new THREE.Mesh(geometry, material);

        // Random positioning
        mesh.position.x = (Math.random() - 0.5) * 40;
        mesh.position.y = (Math.random() - 0.5) * 40;
        mesh.position.z = (Math.random() - 0.5) * 40;

        // Random scaling
        const scale = Math.random() * 2 + 0.5;
        mesh.scale.setScalar(scale);

        scene.add(mesh);

        shapes.push({
            mesh: mesh,
            rotationSpeed: {
                x: (Math.random() - 0.5) * 0.02,
                y: (Math.random() - 0.5) * 0.02,
                z: (Math.random() - 0.5) * 0.01
            },
            floatSpeed: Math.random() * 0.001 + 0.0005,
            floatRadius: Math.random() * 2 + 1
        });
    }

    // Lighting System - Green & White Theme
    const light1 = new THREE.PointLight(0x2ecc71, 2, 100);
    light1.position.set(10, 10, 10);
    scene.add(light1);

    const light2 = new THREE.PointLight(0xffffff, 1.5, 100);
    light2.position.set(-10, -10, -10);
    scene.add(light2);

    const light3 = new THREE.PointLight(0x27ae60, 1.5, 100);
    light3.position.set(0, 15, 5);
    scene.add(light3);

    const ambientLight = new THREE.AmbientLight(0x2c5364, 0.5);
    scene.add(ambientLight);

    // Particles System
    const particlesGeometry = new THREE.BufferGeometry();
    const particlesCount = 500;
    const posArray = new Float32Array(particlesCount * 3);

    for (let i = 0; i < particlesCount * 3; i++) {
        posArray[i] = (Math.random() - 0.5) * 100;
    }

    particlesGeometry.setAttribute(
        'position',
        new THREE.BufferAttribute(posArray, 3)
    );

    const particlesMaterial = new THREE.PointsMaterial({
        size: 0.1,
        color: 0x2ecc71,
        transparent: true,
        opacity: 0.6,
        blending: THREE.AdditiveBlending
    });

    const particlesMesh = new THREE.Points(
        particlesGeometry,
        particlesMaterial
    );
    scene.add(particlesMesh);

    // Camera positioning
    camera.position.z = 20;

    // Mouse interaction
    let mouseX = 0;
    let mouseY = 0;
    let targetX = 0;
    let targetY = 0;

    document.addEventListener('mousemove', (event) => {
        mouseX = (event.clientX / window.innerWidth) * 2 - 1;
        mouseY = -(event.clientY / window.innerHeight) * 2 + 1;
    });

    // Animation loop
    function animate() {
        requestAnimationFrame(animate);

        const time = Date.now() * 0.001;

        // Animate geometric shapes
        shapes.forEach((shape, index) => {
            // Rotation
            shape.mesh.rotation.x += shape.rotationSpeed.x;
            shape.mesh.rotation.y += shape.rotationSpeed.y;
            shape.mesh.rotation.z += shape.rotationSpeed.z;

            // Floating animation
            shape.mesh.position.y +=
                Math.sin(time * shape.floatSpeed + index) * 0.02;
            shape.mesh.position.x +=
                Math.cos(time * shape.floatSpeed + index) * 0.01;
        });

        // Animate particles
        particlesMesh.rotation.y += 0.0005;
        particlesMesh.rotation.x = time * 0.0002;

        // Camera smooth follow mouse
        targetX += (mouseX * 3 - targetX) * 0.05;
        targetY += (mouseY * 3 - targetY) * 0.05;

        camera.position.x = targetX;
        camera.position.y = targetY;
        camera.lookAt(scene.position);

        // Animate lights
        light1.position.x = Math.sin(time * 0.5) * 15;
        light1.position.y = Math.cos(time * 0.3) * 15;
        light1.position.z = Math.sin(time * 0.4) * 10;

        light2.position.x = Math.cos(time * 0.4) * 12;
        light2.position.y = Math.sin(time * 0.5) * 12;
        light2.position.z = Math.cos(time * 0.3) * 8;

        light3.position.z = Math.sin(time * 0.3) * 15;
        light3.position.x = Math.cos(time * 0.2) * 10;

        // Pulsating light intensity
        light1.intensity = 2 + Math.sin(time * 2) * 0.5;
        light2.intensity = 1.5 + Math.cos(time * 1.5) * 0.3;

        renderer.render(scene, camera);
    }

    animate();

    // Handle window resize
    function onWindowResize() {
        camera.aspect = window.innerWidth / window.innerHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(window.innerWidth, window.innerHeight);
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    }

    window.addEventListener('resize', onWindowResize);

    // Cleanup function
    return () => {
        window.removeEventListener('resize', onWindowResize);
        renderer.dispose();
        if (container.contains(renderer.domElement)) {
            container.removeChild(renderer.domElement);
        }
    };
}

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initWebGLBackground);
} else {
    initWebGLBackground();
}