import styled from 'styled-components';
import { Activity, Disc, FileCode, History, Settings } from 'lucide-react';

const NavContainer = styled.nav`
  width: 260px;
  background-color: var(--primary-color);
  color: white;
  padding: 24px 0;
  display: flex;
  flex-direction: column;
  height: 100%;
  box-shadow: 2px 0 5px rgba(0, 0, 0, 0.1);
`;

const NavHeader = styled.div`
  padding: 0 24px;
  margin-bottom: 40px;

  h2 {
    font-size: 1.5rem;
    font-weight: 700;
    letter-spacing: -0.5px;
    margin-bottom: 4px;
  }

  p {
    font-size: 0.75rem;
    opacity: 0.6;
  }
`;

const NavList = styled.ul`
  flex: 1;
`;

const NavItem = styled.li<{ active?: boolean }>`
  display: flex;
  align-items: center;
  padding: 14px 24px;
  cursor: pointer;
  transition: all 0.2s ease;
  background-color: ${props => props.active ? 'rgba(255, 255, 255, 0.1)' : 'transparent'};
  border-left: 4px solid ${props => props.active ? 'var(--accent-color)' : 'transparent'};

  &:hover {
    background-color: rgba(255, 255, 255, 0.05);
  }

  svg {
    margin-right: 12px;
    width: 20px;
    height: 20px;
    color: ${props => props.active ? 'var(--accent-color)' : 'inherit'};
  }

  span {
    font-size: 0.95rem;
    font-weight: ${props => props.active ? '600' : '400'};
  }
`;

const NavFooter = styled.div`
  padding: 24px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
`;

interface SideNavigationProps {
  activeMenu: string;
  onMenuChange: (menu: string) => void;
}

const SideNavigation = ({ activeMenu, onMenuChange }: SideNavigationProps) => {
  const menuItems = [
    { id: 'motion', label: 'Motion Control', icon: Activity },
    { id: 'disk', label: 'Disk Management', icon: Disc },
    { id: 'ncfile', label: 'NC File Queue', icon: FileCode },
    { id: 'history', label: 'Job History', icon: History },
    { id: 'settings', label: 'Settings', icon: Settings },
  ];

  return (
    <NavContainer>
      <NavHeader>
        <h2>Pnc Prime</h2>
        <p>Testing Dashboard v1.0</p>
      </NavHeader>

      <NavList>
        {menuItems.map((item) => {
          const Icon = item.icon;
          return (
            <NavItem 
              key={item.id} 
              active={activeMenu === item.id}
              onClick={() => onMenuChange(item.id)}
            >
              <Icon />
              <span>{item.label}</span>
            </NavItem>
          );
        })}
      </NavList>

      <NavFooter>
        <div style={{ fontSize: '0.7rem', opacity: 0.5 }}>
          © 2026 PncNext Project
        </div>
      </NavFooter>
    </NavContainer>
  );
};

export default SideNavigation;
