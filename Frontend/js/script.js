document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('registerForm').addEventListener('submit', async (event) => {
        event.preventDefault();
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;

        const response = await fetch('https://localhost:7207/api/User/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        if (response.ok) {
            alert('User registered successfully');
            window.location.href = 'login.html';
        } else {
            alert('Error registering user');
        }
    });

    document.getElementById('loginForm').addEventListener('submit', async (event) => {
        event.preventDefault();
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;

        const response = await fetch('https://localhost:7207/api/User/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        if (response.ok) {
            const data = await response.json();
            localStorage.setItem('token', data.token);
            window.location.href = 'greetings.html';
        } else {
            alert('Invalid email or password');
        }
    });

    document.addEventListener('DOMContentLoaded', async () => {
        const token = localStorage.getItem('token');
        if (!token) {
            window.location.href = 'login.html';
            return;
        }

        const response = await fetch('https://localhost:7207/api/HelloGreeting/get-greetings', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            const data = await response.json();
            const greetingsDiv = document.getElementById('greetings');
            data.data.forEach(greeting => {
                const greetingDiv = document.createElement('div');
                greetingDiv.innerHTML = `
                    <p>${greeting.message}</p>
                    <button onclick="editGreeting(${greeting.id})">Edit</button>
                    <button onclick="deleteGreeting(${greeting.id})">Delete</button>
                `;
                greetingsDiv.appendChild(greetingDiv);
            });
        } else {
            alert('Error fetching greetings');
        }
    });

    async function editGreeting(id) {
        const newMessage = prompt('Enter new message:');
        if (newMessage) {
            const token = localStorage.getItem('token');
            const response = await fetch(`https://localhost:7207/api/HelloGreeting/update-greeting/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ newMessage })
            });

            if (response.ok) {
                alert('Greeting updated successfully');
                window.location.reload();
            } else {
                alert('Error updating greeting');
            }
        }
    }

    async function deleteGreeting(id) {
        const token = localStorage.getItem('token');
        const response = await fetch(`https://localhost:7207/api/HelloGreeting/delete-greeting/${id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (response.ok) {
            alert('Greeting deleted successfully');
            window.location.reload();
        } else {
            alert('Error deleting greeting');
        }
    }
});
