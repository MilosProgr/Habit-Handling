import React, { useState, useRef, useEffect } from "react";
import { FaBell } from "react-icons/fa";
import { FiChevronDown } from "react-icons/fi";
import avatar from "../assets/react.svg";
import { useNavigate } from "react-router-dom";


const Chevron = FiChevronDown as React.FC<{ size?: number; className?: string }>;
const Bell = FaBell as React.FC<{ size?: number; className?: string }>;

const TopBar = () => {
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    const navigate = useNavigate();

    // Zatvori dropdown ako klikneš bilo gde van njega
    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setDropdownOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    const handleProfileClick = () => {
        // Ovde možeš dodati navigaciju na profil ili neki modal
        alert("Navigacija na profil");
        navigate("/profile");
        setDropdownOpen(false);
    };

    const handleLogoutClick = () => {
        // Ovde ide tvoj logout kod ili navigacija na logout
        alert("Logout");

        doLogout()

        setDropdownOpen(false);
    };

    return (
        <div className="flex justify-between items-center w-full px-6 py-4 bg-white border-b shadow-sm">
            <div></div>

            <div>
                <div className="flex items-center gap-6">
                    <button className="relative text-gray-600 hover:text-black">
                        <Bell size={20} />
                        <span className="absolute top-0 right-0 block h-2 w-2 rounded-full bg-red-500 ring-2 ring-white" />
                    </button>

                    <div className="relative" ref={dropdownRef}>
                        <button
                            onClick={() => setDropdownOpen(!dropdownOpen)}
                            className="flex items-center gap-1 cursor-pointer focus:outline-none"
                            aria-haspopup="true"
                            aria-expanded={dropdownOpen}
                        >
                            <img
                                width={40}
                                height={40}
                                src={avatar}
                                alt="User avatar"
                                className="rounded-full object-cover"
                            />
                            <Chevron size={16} className="text-gray-600" />
                        </button>

                        {dropdownOpen && (
                            <div className="absolute right-0 mt-2 w-40 bg-white border rounded-md shadow-lg z-20">
                                <button
                                    onClick={handleProfileClick}
                                    className="block w-full text-left px-4 py-2 hover:bg-gray-100"
                                >
                                    Profile
                                </button>
                                <button
                                    onClick={handleLogoutClick}
                                    className="block w-full text-left px-4 py-2 hover:bg-gray-100"
                                >
                                    Logout
                                </button>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default TopBar;

function doLogout() {
    throw new Error("Function not implemented.");
}
