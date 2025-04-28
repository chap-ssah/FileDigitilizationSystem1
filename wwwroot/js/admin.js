document.addEventListener('DOMContentLoaded', function () {
    // Status toggle handler
    document.querySelectorAll('.toggle-status').forEach(btn => {
        btn.addEventListener('click', function () {
            const userId = this.dataset.userid;
            const action = this.dataset.action;
            const activate = action === 'activate';

            // Fix the URL formatting (remove spaces)
            fetch(`/Admin/ToggleUserStatus?userId=${userId}&activate=${activate}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            })
                .