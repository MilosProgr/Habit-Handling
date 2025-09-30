// src/features/profile/ProfilePage.tsx
import { useEffect, useState } from "react";
import { useKeycloak } from "@react-keycloak/web";

interface UserProfile {
    id: string;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
}

interface UpdateProfileDto {
    firstName: string;
    lastName: string;
    email: string;
}

export const ProfilePage = () => {
    const { keycloak } = useKeycloak();
    const [user, setUser] = useState<UserProfile | null>(null);
    const [formData, setFormData] = useState<UpdateProfileDto>({
        firstName: "",
        lastName: "",
        email: "",
    });
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Fetch current user profile
    useEffect(() => {
        const fetchProfile = async () => {
            if (!keycloak?.token) return;

            try {
                const res = await fetch("/api/users/me", {
                    headers: {
                        Authorization: `Bearer ${keycloak.token}`,
                    },
                });

                if (!res.ok) throw new Error("Failed to fetch profile");

                const data: UserProfile = await res.json();
                setUser(data);
                setFormData({
                    firstName: data.firstName,
                    lastName: data.lastName,
                    email: data.email,
                });
            } catch (err: unknown) {
                if (err instanceof Error) {
                    setError(err.message);
                } else {
                    setError("An unknown error occurred");
                }
            } finally {
                setLoading(false);
            }
        };

        fetchProfile();
    }, [keycloak?.token]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!keycloak?.token) return;
        setSaving(true);
        setError(null);

        try {
            const res = await fetch("/api/users/me/profile", {
                method: "PUT",
                headers: {
                    Authorization: `Bearer ${keycloak.token}`,
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(formData),
            });

            if (!res.ok) {
                const msg = await res.text();
                throw new Error(msg || "Failed to update profile");
            }

            alert("Profile updated successfully!");
        } catch (err: unknown) {
            if (err instanceof Error) {
                setError(err.message);
            } else {
                setError("An unknown error occurred");
            }
        } finally {
            setSaving(false);
        }
    };

    if (loading) return <div>Loading profile...</div>;
    if (error) return <div className="text-red-500">Error: {error}</div>;
    if (!user) return <div>No profile found.</div>;

    return (
        <div className="max-w-md mx-auto p-4">
            <h1 className="text-2xl font-bold mb-4">My Profile</h1>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block font-medium">Username</label>
                    <input
                        type="text"
                        value={user.username}
                        disabled
                        className="border p-2 w-full bg-gray-100"
                    />
                </div>
                <div>
                    <label className="block font-medium">Email</label>
                    <input
                        type="email"
                        name="email"
                        value={formData.email}
                        onChange={handleChange}
                        className="border p-2 w-full"
                    />
                </div>
                <div>
                    <label className="block font-medium">First Name</label>
                    <input
                        type="text"
                        name="firstName"
                        value={formData.firstName}
                        onChange={handleChange}
                        className="border p-2 w-full"
                    />
                </div>
                <div>
                    <label className="block font-medium">Last Name</label>
                    <input
                        type="text"
                        name="lastName"
                        value={formData.lastName}
                        onChange={handleChange}
                        className="border p-2 w-full"
                    />
                </div>
                <button
                    type="submit"
                    disabled={saving}
                    className="bg-blue-500 text-white px-4 py-2 rounded"
                >
                    {saving ? "Saving..." : "Update Profile"}
                </button>
            </form>
        </div>
    );
};
