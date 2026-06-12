import styled from 'styled-components';

const Panel = styled.div`
  background-color: var(--panel-bg);
  border-radius: 8px;
  padding: 20px;
  border: 1px solid var(--border-color);
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
`;

const PanelTitle = styled.div`
  font-size: 16px;
  font-weight: 700;
  margin-bottom: 15px;
  color: var(--primary-color);
  display: flex;
  align-items: center;
  gap: 8px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 10px;
`;

const ConsoleRow = styled.div`
  display: flex;
  gap: 12px;
  margin-bottom: 15px;

  &:last-child { margin-bottom: 0; }
`;

const Select = styled.select`
  background: #f1f3f5;
  color: var(--primary-color);
  border: 1px solid var(--border-color);
  width: 85px;
  padding: 8px;
  border-radius: 4px;
  font-weight: bold;
  cursor: pointer;
`;

const Input = styled.input`
  flex: 1;
  background: #222; /* Keep input area dark for distinct G-code feeling */
  color: #00ff00;
  border: 1px solid #000;
  padding: 10px 15px;
  font-family: 'Consolas', monospace;
  border-radius: 4px;
  font-size: 15px;

  &::placeholder { color: #006600; opacity: 0.7; }
`;

const ActionButton = styled.button`
  background: var(--accent-color);
  color: white;
  width: 80px;
  padding: 8px;
  border-radius: 4px;
  font-weight: 800;
  transition: opacity 0.2s;
  
  &:hover { opacity: 0.9; }
`;

const ModeBadge = styled.div`
  background: #34495e;
  color: white;
  border: 1px solid #2c3e50;
  width: 85px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  font-weight: bold;
  font-size: 13px;
`;

const CommandConsole = () => {
  return (
    <Panel>
      <PanelTitle>💻 Command Console</PanelTitle>
      <ConsoleRow>
        <Select>
          <option>MMA</option>
          <option>MMI</option>
        </Select>
        <Input placeholder="X Y Z A B (e.g. X100.0 Y-45.5)" />
        <ActionButton>GO</ActionButton>
      </ConsoleRow>
      <ConsoleRow>
        <ModeBadge>MDA</ModeBadge>
        <Input placeholder="Single G-Code line... (e.g. G0 X0)" />
        <ActionButton>RUN</ActionButton>
      </ConsoleRow>
    </Panel>
  );
};

export default CommandConsole;
